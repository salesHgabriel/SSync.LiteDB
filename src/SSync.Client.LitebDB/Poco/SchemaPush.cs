using SSync.Client.LitebDB.Abstractions.Sync;
using System.Text.Json.Serialization;

namespace SSync.Client.LitebDB.Poco
{
    public class SchemaPush<T> where T : BaseSync
    {
        public string? Collection { get; set; }
        public long Timestamp { get; set; }
        public Change Changes { get; set; } = default!;

        [JsonIgnore]
        public bool HasChanges => Changes is not null && (Changes.Created.Any() || Changes.Updated.Any() || Changes.Deleted.Any());

        [JsonIgnore]
        public bool HasDeleted => Changes?.Deleted?.Any() ?? false;

        public void SetCommitDatabaseOperation(bool succeced) => CommitedDatabaseOperation = succeced;

        [JsonIgnore]
        public bool CommitedDatabaseOperation { get; private set; }

        public class Change
        {
            public IEnumerable<T> Created { get; set; } = default!;
            public IEnumerable<T> Updated { get; set; } = default!;
            public IEnumerable<Guid> Deleted { get; set; } = default!;
        }
    }
}