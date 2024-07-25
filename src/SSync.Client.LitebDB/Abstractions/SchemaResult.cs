namespace SSync.Client.LitebDB.Abstractions
{
    public class SchemaResult<T> where T : BaseSync
    {
        public SchemaResult()
        {

        }
        public SchemaResult(string? documentName, long timestamp, Change changesModel)
        {
            Document = documentName;
            Timestamp = timestamp;
            Changes = changesModel;
        }

        public string? Document { get; private set; }
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
