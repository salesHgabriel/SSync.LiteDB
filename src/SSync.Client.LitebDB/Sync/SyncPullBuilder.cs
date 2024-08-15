using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Sync;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPullBuilder : IBuilder
    {
        private readonly List<Func<object>> _actions = new List<Func<object>>();
        public List<object> DatabaseLocalChanges { get; } = new List<object>();

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
    }
}