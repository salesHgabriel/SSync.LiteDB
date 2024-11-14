using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Abstractions
{
    public abstract class ISchema
    {
        protected ISchema(Guid id) : base()
        {
            Id = id;
        }

        protected ISchema(Guid id, Time? time) : base()
        {
            Id = id;
            CreateAt(time);
            UpdateAt(time);
        }

        public Guid Id { get; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }


        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void CreateAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow : DateTime.Now;
            CreatedAt = now;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void UpdateAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow : DateTime.Now;
            UpdatedAt = now;
        }

        /// <summary>
        /// if datetime is null set value in utc
        /// </summary>
        /// <param name="now"></param>
        public void DeleteAt(Time? time)
        {
            time ??= Time.UTC;

            var now = time == Time.UTC ? DateTime.UtcNow : DateTime.Now;

            DeletedAt = now;
        }
    }
}