using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Abstractions
{

    public abstract class ISSyncEntityRoot
    {
        /// <summary>
        /// Default set datetime utc
        /// </summary>
        protected ISSyncEntityRoot()
        {
            Id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }
        protected ISSyncEntityRoot(Time time)
        {
            var now = DateTime.Now;
            var nowUTC = DateTime.UtcNow;

            Id = Guid.NewGuid();
            CreatedAt = time == Time.LOCAL_TIME ? now : nowUTC;
            UpdatedAt = time == Time.LOCAL_TIME ? now : nowUTC;
        }
        protected ISSyncEntityRoot(Guid id, Time time)
        {
            var now = DateTime.Now;
            var nowUTC = DateTime.UtcNow;
            Id = id;
            CreatedAt = time == Time.LOCAL_TIME ? now : nowUTC;
            UpdatedAt = time == Time.LOCAL_TIME ? now : nowUTC;
        }

        protected ISSyncEntityRoot(Guid id, DateTime date)
        {
            Id = id;
            CreatedAt = date;
            UpdatedAt = date;
        }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public void SetUpdatedAt(DateTime dateTime) => UpdatedAt = dateTime;
        public void SetDeletedAt(DateTime dateTime) => DeletedAt = dateTime;

    }
}
