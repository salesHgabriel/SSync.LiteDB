using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Sync;
using SSync.Client.LitebDB.Poco;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPushBuilder : IBuilder
    {
        private readonly List<Func<object>> _actions = [];
        public readonly JsonArray _databaseRemoteChanges;

        public SyncPushBuilder(string databaseRemoteChanges)
        {
            ArgumentException.ThrowIfNullOrEmpty(databaseRemoteChanges);
            _databaseRemoteChanges = JsonSerializer.Deserialize<JsonArray>(databaseRemoteChanges)!;
        }

        public SyncPushBuilder AddPushSchemaSync<T>(Func<SchemaPush<T>, SchemaPush<T>> action, string collectionName) where T : BaseSync
        {
            ArgumentNullException.ThrowIfNull(action);

            var filteredNode = _databaseRemoteChanges.First(node => node![nameof(SchemaPush<T>.Collection)]?.ToString() == collectionName)!;

            var doc = JsonSerializer.Deserialize<SchemaPush<T>>(filteredNode.ToJsonString())!;

            _actions.Add(() => action(doc));

            return this;
        }

        public void Build()
        {
            foreach (var action in _actions)
            {
                action();
            }
            _actions.Clear();
        }
    }
}