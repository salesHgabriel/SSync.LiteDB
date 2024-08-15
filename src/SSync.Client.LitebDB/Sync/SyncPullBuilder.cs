using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Sync;
using System.Text.Json;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPullBuilder : IBuilder
    {
        private readonly List<Func<object>> _actions = new List<Func<object>>();
        public List<object> DatabaseLocalChanges { get; } = new List<object>();
        public string? JsonDatabaseLocalChanges { get; }

        public SyncPullBuilder AddPullSync<T>(Func<SchemaPullResult<T>> action) where T : SchemaSync
        {
            _actions.Add(() => action());
            return this;
        }

        public void Build()
        {
            DatabaseLocalChanges.Clear();
            foreach (var act in _actions)
            {
                DatabaseLocalChanges.Add(act());
            }

            _actions.Clear();
        }

        public string? GetChangesToJson(JsonSerializerOptions? opt =null)
        {
            return DatabaseLocalChanges is not null && DatabaseLocalChanges.Count != 0
                ? JsonSerializer.Serialize(DatabaseLocalChanges, opt)
                : string.Empty;
        }
    }
}