using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;
using SSync.Server.LitebDB.Engine.Builders;
using SSync.Shared.ClientServer.LitebDB.Exceptions;
using System.Reflection;

namespace SSync.Server.LitebDB.Sync
{
    public class SchemaCollection : ISchemaCollection
    {
        private readonly ISSyncServices _syncServices;

        private readonly ExecutionOrderBuilder _builder;

        public SchemaCollection(ISSyncServices syncServices, ExecutionOrderBuilder builder)
        {
            _syncServices = syncServices;
            _builder = builder;
        }

        public async Task<SchemaPullResult<TCollection>> CheckChanges<TCollection, TParamenter>(TParamenter paramenter, SSyncOptions? options = null)
                where TCollection : ISchema
                where TParamenter : SSyncParamenter
        {
            if (paramenter.Colletions.Length == 0) throw new PullChangesException("You need set collections");

            DateTime lastPulledAt = DateTimeOffset.FromUnixTimeMilliseconds(paramenter.Timestamp).DateTime;

            var timestamp = options?.TimeConfig == Time.UTC ? DateTime.UtcNow : DateTime.Now;

            var handler = _syncServices.PullRequestHandler<TCollection, TParamenter>();

            var query = handler.Query(paramenter);

            if (paramenter.Timestamp == 0)
            {
                var createds = (await query)
                    .Where(entity => entity.DeletedAt == null)
                    .ToList();

                var updateds = Enumerable.Empty<TCollection>();
                var deleteds = new List<Guid>();

                return new SchemaPullResult<TCollection>(paramenter.CurrentColletion, timestamp.Ticks, new SchemaPullResult<TCollection>.Change(createds, updateds, deleteds));
            }

            return new SchemaPullResult<TCollection>(
                paramenter.CurrentColletion,
                timestamp.Ticks,
                new SchemaPullResult<TCollection>.Change(
                   (await query).Where(d => d.CreatedAt > lastPulledAt).ToList(),
                   (await query).Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.UpdatedAt > lastPulledAt).ToList(),
                   (await query).Where(d => d.CreatedAt <= lastPulledAt).Where(d => d.DeletedAt > lastPulledAt).Select(d => d.Id).ToList()
                    )
                );
        }

        public async Task<List<object>> PullChangesAsync(SSyncParamenter parameter, SSyncOptions? options = null)
        {
            var result = new List<object>();

            var steps = _builder.GetSteps();

            foreach (var step in steps.Where(s => parameter.Colletions.Contains(s.Parameter)))
            {
                parameter.CurrentColletion = step.Parameter;

                var method = typeof(SchemaCollection)
                    .GetMethod(nameof(CheckChanges), BindingFlags.Instance | BindingFlags.Public)
                    .MakeGenericMethod(step.SyncType, parameter.GetType());

                var task = (Task)method.Invoke(this, new object[] { parameter, options });
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");

                result.Add(resultProperty.GetValue(task));
            }

            return result;
        }
    }
}