using LiteDB;

namespace SSync.Client.LitebDB.Abstractions
{
    public abstract class BaseSync
    {
        protected BaseSync(Guid id) : base() => Id = id;

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void CreateAt(DateTime? now = null)
        {
            CreatedAt = now is null ? DateTime.UtcNow.Ticks : now.Value.Ticks;
            Status = StatusSync.CREATED;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void UpdateAt(DateTime? now = null)
        {
            UpdatedAt = now is null ? DateTime.UtcNow.Ticks : now.Value.Ticks;
            Status = StatusSync.UPDATED;
        }


        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void DeleteAt(DateTime? now = null)
        {
            DeletedAt = now is null ? DateTime.UtcNow.Ticks : now.Value.Ticks;
            Status = StatusSync.DELETED;
        }

        [BsonId]
        public Guid Id { get; private set; }

        public long CreatedAt { get; private set; }
        public long UpdatedAt { get; private set; }
        public long? DeletedAt { get; private set; }
        public StatusSync Status { get; private set; }
    }
}