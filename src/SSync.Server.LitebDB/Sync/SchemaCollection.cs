﻿using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;
using SSync.Server.LitebDB.Enums;
using SSync.Server.LitebDB.Exceptions;
using SSync.Server.LitebDB.Extensions;
using SSync.Server.LitebDB.Poco;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSync.Server.LitebDB.Sync
{
    public class SchemaCollection : ISchemaCollection, IDisposable
    {
        private readonly ISSyncServices _syncServices;
        private readonly ISSyncDbContextTransaction _sSyncDbContextTransaction;
        private IPullExecutionOrderStep? _pullBuilder;

        //TODO: test: USE SINGLETON
        private IPushExecutionOrderStep? _pushBuilder;

        private SSyncOptions? _options = null;
        private bool _disposedValue;

        public SchemaCollection(ISSyncServices syncServices,
            IPullExecutionOrderStep pullBuilder,
            IPushExecutionOrderStep pushBuilder,
            ISSyncDbContextTransaction sSyncDbContextTransaction)
        {
            _syncServices = syncServices;
            _pullBuilder = pullBuilder;
            _pushBuilder = pushBuilder;
            _sSyncDbContextTransaction = sSyncDbContextTransaction;
        }

        public async Task<List<object>> PullChangesAsync(SSyncParameter parameter, SSyncOptions? options = null)
        {
            if (parameter.Colletions.Length == 0)
            {
                Log($"Error collection is required", consoleColor: ConsoleColor.Red);

                throw new PullChangesException("You need set collections");
            }

            _options = options;

            var result = new List<object>();

            Log($"Start pull changes");
            var steps = _pullBuilder?.GetSteps();

            if (steps is not null)
            {
                foreach (var (SyncType, Parameter) in steps.Where(s => parameter.Colletions.Contains(s.Parameter)))
                {
                    parameter.CurrentColletion = Parameter;

                    Log($"Start pull changes of collection {Parameter}");

                    MethodInfo? method = typeof(SchemaCollection)!
                        .GetMethod(nameof(CheckChanges), BindingFlags.Instance | BindingFlags.NonPublic)!
                        .MakeGenericMethod(SyncType, parameter.GetType()) ?? throw new PullChangesException("Not found pull request handler");

                    var task = (Task)method.Invoke(this, [parameter])!;

                    if (task is not null)
                    {
                        await task.ConfigureAwait(false);

                        var resultProperty = task.GetType().GetProperty("Result");

                        var collectionResult = resultProperty?.GetValue(task);

                        if (collectionResult is not null)
                        {
                            result.Add(collectionResult);
                        }
                    }
                }
            }

            return result;
        }

        public async IAsyncEnumerable<object> PullStreamChanges(SSyncParameter parameter, SSyncOptions? options = null)
        {
            if (parameter.Colletions.Length == 0)
            {
                Log($"Error collection is required", consoleColor: ConsoleColor.Red);

                throw new PullChangesException("You need set collections");
            }

            _options = options;

            Log($"Start pull changes delta");
            var steps = _pullBuilder?.GetSteps();

            if (steps is not null)
            {
                foreach (var (SyncType, Parameter) in steps.Where(s => parameter.Colletions.Contains(s.Parameter)))
                {
                    parameter.CurrentColletion = Parameter;

                    Log($"Start pull changes of collection {Parameter}");

                    MethodInfo? method = typeof(SchemaCollection)!
                        .GetMethod(nameof(CheckChanges), BindingFlags.Instance | BindingFlags.NonPublic)!
                        .MakeGenericMethod(SyncType, parameter.GetType()) ?? throw new PullChangesException("Not found pull request handler");

                    var task = (Task)method.Invoke(this, [parameter])!;

                    if (task is not null)
                    {
                        await task.ConfigureAwait(false);

                        var resultProperty = task.GetType().GetProperty("Result");

                        var collectionResult = resultProperty?.GetValue(task);

                        if (collectionResult is not null)
                        {
                            yield return collectionResult;
                        }
                    }
                }
            }
        }

        public async Task<DateTime> PushChangesAsync(JsonArray changes, SSyncParameter parameter, SSyncOptions? optionsSync = null)
        {
            _options = optionsSync;

            if (changes == null || changes.Count == 0)
            {
                Log("changes is required", consoleColor: ConsoleColor.Red);

                throw new PushChangeException("Changes is array cannot be null or empty.");
            }
            try
            {
                await _sSyncDbContextTransaction.BeginTransactionSyncAsync();

                Log("Open Transaction database");

                var schemaChanges = await ExecuteChanges(changes, parameter);

                await _sSyncDbContextTransaction.CommitTransactionSyncAsync();

                Log("Commit Transaction database");

                return schemaChanges;
            }
            catch (Exception ex)
            {
                Log($"Push changes not working", consoleColor: ConsoleColor.Red);

                Log(ex, consoleColor: ConsoleColor.Red);

                Log(ex.Message, consoleColor: ConsoleColor.Red);

                Log(ex.InnerException?.Message ?? string.Empty, consoleColor: ConsoleColor.Red);

                await _sSyncDbContextTransaction.RollbackTransactionSyncAsync();
                throw new PushChangeException(ex.Message);
            }
        }

        private async Task<SchemaPullResult<TCollection>> CheckChanges<TCollection, TParamenter>(TParamenter paramenter)
        where TCollection : ISchema
        where TParamenter : SSyncParameter
        {
            if (paramenter.Colletions.Length == 0)
            {
                Log($"Error collection is required", consoleColor: ConsoleColor.Red);

                throw new PullChangesException("You need set collections");
            }

            if (string.IsNullOrEmpty(paramenter.CurrentColletion))
            {
                Log($"Not found collection {paramenter.CurrentColletion}", consoleColor: ConsoleColor.Red);

                throw new PullChangesException("Not found collection");
            }

            var timestamp = _options?.TimeConfig == Time.LOCAL_TIME ? DateTime.Now : DateTime.UtcNow;

            var handler = _syncServices.PullRequestHandler<TCollection, TParamenter>();

            var query = await handler.QueryAsync(paramenter);

            if (DateTimeExtension.IsFirstPull(paramenter.Timestamp))
            {
                var createds = query
                    .Where(entity => entity.DeletedAt == null)
                    .ToList();

                var updateds = Enumerable.Empty<TCollection>();
                var deleteds = new List<Guid>();

                var allChanges = new SchemaPullResult<TCollection>(paramenter.CurrentColletion, timestamp, new SchemaPullResult<TCollection>.Change(createds, updateds, deleteds));

                Log($"Successfully pull changes all database");

                return allChanges;
            }

            DateTime lastPulledAt = paramenter.Timestamp;

            var syncTimestamp = timestamp;

            var changesOfTime = new SchemaPullResult<TCollection>(
                paramenter.CurrentColletion,
                syncTimestamp,
                new SchemaPullResult<TCollection>.Change(

                   created: query
                            .Where(d => d.CreatedAt >= lastPulledAt)
                            .Where(d => d.UpdatedAt == d.CreatedAt)
                            .Where(d => !d.DeletedAt.HasValue)
                            .ToList(),

                   updated: query
                        .Where(d => d.UpdatedAt >= lastPulledAt)
                        .Where(d => d.UpdatedAt != d.CreatedAt)
                        .Where(d => !d.DeletedAt.HasValue)
                        .ToList(),

                   deleted: query
                        .Where(d => d.DeletedAt >= lastPulledAt)
                        .Where(d => d.DeletedAt.HasValue)
                        .Select(d => d.Id)
                        .Distinct()
                        .ToList()
                ));

            Log($"Successfully pull changes database of time {lastPulledAt}");

            return changesOfTime;
        }

        private async Task<DateTime> ExecuteChanges(JsonArray changes, SSyncParameter parameter)
        {
            try
            {
                Log($"Start push changes");

                ArgumentNullException.ThrowIfNull(changes);

                var schemasSync = _pushBuilder?.GetSchemas();

                if (schemasSync is not null)
                {
                    var parseChangesMethod = GetType().GetMethod(nameof(ParseChanges), BindingFlags.Instance | BindingFlags.NonPublic);

                    ArgumentNullException.ThrowIfNull(parseChangesMethod);

                    var changesMap = new Dictionary<string, SchemaPush<ISchema>>();

                    foreach (var changeObj in changes)
                    {
                        if (changeObj is null)
                        {
                            return DateTime.MinValue;
                        }
                        var collectionName = changeObj!["Collection"]?.ToString() ?? changeObj["collection"]?.ToString();

                        Log($"Start {collectionName}");

                        if (!string.IsNullOrEmpty(collectionName) && schemasSync.TryGetValue(collectionName, out var schemaType))
                        {
                            var genericMethodParseChanges = parseChangesMethod.MakeGenericMethod(schemaType);

                            var task = (Task<bool>?)genericMethodParseChanges.Invoke(this, [changeObj, parameter]) ?? throw new PushChangeException("task of method not found");

                            if (!await task)
                            {
                                Log($"Error in parse change of collection {collectionName} to type {schemaType.Name}", consoleColor: ConsoleColor.Red);

                                Log($"Not push changes", consoleColor: ConsoleColor.Red);

                                // throw new PushChangeException("Not push changes");
                            }
                        }
                    }
                }

                return _options?.TimeConfig == Time.UTC ? DateTime.UtcNow : DateTime.Now;
            }
            catch (Exception ex)
            {
                Log($"Error process execute push changes", consoleColor: ConsoleColor.Red);

                Log(ex.Message, consoleColor: ConsoleColor.Red);

                Log(ex.InnerException?.Message ?? string.Empty, consoleColor: ConsoleColor.Red);

                throw new PushChangeException(ex.Message);
            }
        }

        private async Task<bool> ParseChanges<TSchema>(JsonNode nodeChange, SSyncParameter parameter)
            where TSchema : ISchema
        {
            var jsonChange = nodeChange.ToJsonString();

            var schemaPush = JsonSerializer.Deserialize<SchemaPush<TSchema>>(jsonChange, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var result = true;

            if (schemaPush == null || !schemaPush.HasChanges)
            {
                result = false;
                return result;
            }

            var requestHandler = _syncServices.PushRequestHandler<TSchema>();

            var lastPulledAtSync = DateTimeExtension.ParseDaTimeFromConfig(parameter.Timestamp, _options?.TimeConfig);

            if (schemaPush.Changes.Created.Any())
            {
                foreach (var change in schemaPush.Changes.Created)
                {
                    if (!await requestHandler.UpsertAsync(change, lastPulledAtSync, _options?.TimeConfig))
                        result = false;
                }
            }

            if (schemaPush.Changes.Updated.Any())
            {
                foreach (var change in schemaPush.Changes.Updated)
                {
                    if (!await requestHandler.UpsertAsync(change, lastPulledAtSync, _options?.TimeConfig))
                        result = false;
                }
            }

            if (schemaPush.Changes.Deleted.Any())
            {
                foreach (var change in schemaPush.Changes.Deleted)
                {
                    if (!await requestHandler.DeleteAsync(change, lastPulledAtSync, _options?.TimeConfig))
                        result = false;
                }
            }

            return result;
        }

        private void Log(object logMessage, string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Green)
        {
            if (_options?.Mode == Mode.DEBUG)
            {
                if (_options.SaveLogOnFile)
                {
                    using TextWriter w = File.AppendText($"{_options?.PathFile}\\{title}");
                    w.Write("\r\nLog Entry :");
                    w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    w.WriteLine("  :");
                    w.WriteLine($"  :{logMessage}");
                    w.WriteLine("########################################################################");
                }
                else
                {
                    var msg = new StringBuilder();
                    msg.Append("\r\nLog Entry :");
                    msg.AppendLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    msg.AppendLine("  :");
                    msg.AppendLine($"  :{logMessage}");
                    msg.AppendLine("########################################################################");

                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine(msg.ToString());
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _pullBuilder = null;
                    _pushBuilder = null;
                    _options = null;
                }

                _disposedValue = true;
            }
        }
    }
}