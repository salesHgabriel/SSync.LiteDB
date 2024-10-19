using LiteDB;
using SSync.Client.LitebDB.Enums;
using SSync.Client.LitebDB.Extensions;
using System;
using System.Text.Json.Serialization;

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
        public void CreateAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow.ToUniversalTime() : DateTime.Now.ToLocalTime();
            CreatedAt = now.ToUnixTimestamp(time);
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
            UpdatedAt = now.ToUnixTimestamp(time);
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

            DeletedAt = now.ToUnixTimestamp(time);
            Status = StatusSync.DELETED;
        }

        [BsonId]
        public Guid Id { get; set; }

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public long? DeletedAt { get; set; }

        [JsonIgnore]
        public StatusSync Status { get; set; } = StatusSync.CREATED;
    }
}