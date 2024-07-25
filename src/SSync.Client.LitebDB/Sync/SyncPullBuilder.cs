using SSync.Client.LitebDB.Abstractions;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPullBuilder : ISyncPullBuilder
    {
        private readonly List<Func<object>> _actions = new List<Func<object>>();
        public List<object> DatabaseChanges { get; } = new List<object>();

        public SyncPullBuilder AddPullSync<T>(Func<SchemaResult<T>> action) where T : BaseSync
        {
            _actions.Add(() => action());
            return this;
        }

        public void Build()
        {
            DatabaseChanges.Clear();
            foreach (var act in _actions)
            {
                DatabaseChanges.Add(act());
            }
        }
    }
}