using SSync.Client.SQLite.Abstractions;
using SSync.Client.SQLite.Abstractions.Sync;
using SSync.Client.SQLite.Extensions;
using SSync.Client.SQLite.Poco;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSync.Client.SQLite.Sync
{
    public class SyncPushBuilder : IBuilder
    {
        private readonly List<Func<Task>>  _actions = [];
        public readonly JsonArray _databaseRemoteChanges;
        public readonly bool _collectionIsUpperCase = false;
        /// <summary>
        /// Initialize builder
        /// </summary>
        /// <param name="databaseRemoteChanges">json all changes</param>
        /// <param name="collectionIsUpperCase">Set property of json is Collection or collection</param>
        public SyncPushBuilder(string databaseRemoteChanges, bool collectionIsUpperCase = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(databaseRemoteChanges);
            _databaseRemoteChanges = JsonSerializer.Deserialize<JsonArray>(databaseRemoteChanges)!;
            _collectionIsUpperCase = collectionIsUpperCase;
        }

        public SyncPushBuilder AddPushSchemaSync<T>(Func<SchemaPush<T>, Task<SchemaPush<T>>> action, string collectionName) where T : SchemaSync
        {
            ArgumentNullException.ThrowIfNull(action);

            string propertyCol = _collectionIsUpperCase ? nameof(SchemaPush<T>.Collection) : nameof(SchemaPush<T>.Collection).ToLower();

            var filteredNode = _databaseRemoteChanges.First(node => node![propertyCol]?.ToString() == collectionName)!;

            var parseJsonPascalCase = filteredNode.ToJsonString().ConvertCamelCaseToPascalCaseJson();

            var doc = JsonSerializer.Deserialize<SchemaPush<T>>(parseJsonPascalCase)!;

            _actions.Add(() => action(doc));

            return this;
        }

        public async Task BuildAsync()
        {
            var tasks = _actions.Select(action => action()).ToList(); // Collect tasks
            await Task.WhenAll(tasks); // Wait for all async operations to complete
            _actions.Clear();
            //foreach (var action in _actions)
            //{
            //    action();
            //}
            //_actions.Clear();

            //return Task.CompletedTask;
        }
    }
}