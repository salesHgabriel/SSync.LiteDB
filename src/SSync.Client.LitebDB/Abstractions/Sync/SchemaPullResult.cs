namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public class SchemaPullResult<T> where T : BaseSync
    {
        public SchemaPullResult()
        {
        }

        public SchemaPullResult(string? collection, long timestamp, Change changesModel)
        {
            Collection = collection;
            Timestamp = timestamp;
            Changes = changesModel;
        }

        public string? Collection { get; private set; }
        public long? Timestamp { get; private set; } = 0;
        public Change Changes { get; private set; } = default!;

        public class Change
        {
            public Change(IEnumerable<T> created, IEnumerable<T> updated, IEnumerable<Guid> deleted)
            {
                Created = created;
                Updated = updated;
                Deleted = deleted;
            }

            public IEnumerable<T> Created { get; private set; } = default!;
            public IEnumerable<T> Updated { get; private set; } = default!;
            public IEnumerable<Guid> Deleted { get; private set; } = default!;
        }
    }
}