using LiteDB;
using SSync.Client.LitebDB.Enums;
using System.Text.Json.Serialization;

namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public abstract class SchemaSync
    {
        protected SchemaSync()
        {
        }

        protected SchemaSync(Guid id) : base()
        {
            Id = id;
        }

        protected SchemaSync(Guid id, Time? time) : base()
        {
            Id = id;

            var now = time == Time.UTC ? DateTime.UtcNow.ToUniversalTime() : DateTime.Now.ToLocalTime();
            CreatedAt = now;
            UpdatedAt = now;
        }

        protected SchemaSync(Guid id, DateTime time) : base()
        {
            Id = id;

            CreatedAt = time;
            UpdatedAt = time;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void CreateAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow.ToUniversalTime() : DateTime.Now.ToLocalTime();
            CreatedAt = now;
            Status = StatusSync.CREATED;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void UpdateAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow.ToUniversalTime() : DateTime.Now.ToLocalTime();
            UpdatedAt = now;
            Status = StatusSync.UPDATED;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void DeleteAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow.ToUniversalTime() : DateTime.Now.ToLocalTime();

            DeletedAt = now;
            Status = StatusSync.DELETED;
        }

        [BsonId]
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public StatusSync Status { get; set; } = StatusSync.CREATED;
    }
}