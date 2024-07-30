using SSync.Client.LitebDB.Abstractions;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPullBuilder : IBuilder
    {
        private readonly List<Func<object>> _actions = new List<Func<object>>();
        public List<object> DatabaseLocalChanges { get; } = new List<object>();

        public SyncPullBuilder AddPullSync<T>(Func<Task<SchemaPullResult<T>>> action) where T : BaseSync
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