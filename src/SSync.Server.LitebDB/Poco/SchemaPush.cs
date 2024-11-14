using SSync.Server.LitebDB.Abstractions;
using System.Text.Json.Serialization;

namespace SSync.Server.LitebDB.Poco
{
    public class SchemaPush<T> where T : ISchema
    {
        public string? Collection { get; set; }
        public DateTime Timestamp { get; set; }
        public Change Changes { get; set; } = default!;

        [JsonIgnore]
        public bool HasChanges => Changes is not null && (Changes.Created.Any() || Changes.Updated.Any() || Changes.Deleted.Any());

        [JsonIgnore]
        public bool HasDeleted => Changes?.Deleted?.Any() ?? false;

        [JsonIgnore]
        public bool HasCreated => Changes?.Created?.Any() ?? false;

        public bool HasUpdated => Changes?.Updated?.Any() ?? false;

        [JsonIgnore]
        public bool CommitedDatabaseOperation { get; private set; }

        public class Change
        {
            public Change()
            {
            }

            public Change(IEnumerable<T> created, IEnumerable<T> updated, IEnumerable<Guid> deleted)
            {
                Created = created;
                Updated = updated;
                Deleted = deleted;
            }

            public IEnumerable<T> Created { get; set; } = default!;
            public IEnumerable<T> Updated { get; set; } = default!;
            public IEnumerable<Guid> Deleted { get; set; } = default!;
        }

        public void SetCommitDatabaseOperation(bool succeced) => CommitedDatabaseOperation = succeced;
    }
}