using LiteDB;
using SSync.Client.LitebDB.Extensions;

namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public abstract class SchemaSync
    {
        protected SchemaSync()
        {
        }

        protected SchemaSync(Guid id) : base() => Id = id;

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void CreateAt(DateTime? now = null)
        {
            CreatedAt = now is null ? DateTime.UtcNow.ToUnixTimestamp() : now.Value.ToUnixTimestamp();
            Status = StatusSync.CREATED;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void UpdateAt(DateTime? now = null)
        {
            UpdatedAt = now is null ? DateTime.UtcNow.ToUnixTimestamp() : now.Value.ToUnixTimestamp();
            Status = StatusSync.UPDATED;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void DeleteAt(DateTime? now = null)
        {
            DeletedAt = now is null ? DateTime.UtcNow.ToUnixTimestamp() : now.Value.ToUnixTimestamp();
            Status = StatusSync.DELETED;
        }

        [BsonId]
        public Guid Id { get; set; }

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public long? DeletedAt { get; set; }

        public StatusSync Status { get; set; }
    }
}