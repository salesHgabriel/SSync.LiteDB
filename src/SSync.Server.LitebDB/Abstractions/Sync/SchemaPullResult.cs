namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public class SchemaPullResult<T> where T : ISchema
    {
        public SchemaPullResult()
        {
        }

        public SchemaPullResult(string? collectionName, DateTime timestamp, Change changesModel)
        {
            Collection = collectionName;
            Timestamp = timestamp;
            Changes = changesModel;
        }

        public string? Collection { get; private set; }
        public DateTime? Timestamp { get; private set; } = DateTime.MinValue;
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