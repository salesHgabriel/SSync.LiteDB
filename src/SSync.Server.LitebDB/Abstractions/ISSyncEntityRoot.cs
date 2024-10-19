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
            CreatedAt = DateTime.UtcNow;
        }
        protected ISSyncEntityRoot(Time time)
        {
            Id = Guid.NewGuid();
            CreatedAt = time == Time.LOCAL_TIME ? DateTime.Now : DateTime.UtcNow;
        }
        protected ISSyncEntityRoot(Guid id, Time time)
        {
            Id = id;
            CreatedAt = time == Time.LOCAL_TIME ? DateTime.Now : DateTime.UtcNow;
        }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public void SetUpdatedAt(DateTime dateTime) => UpdatedAt = dateTime;
        public void SetDeletedAt(DateTime dateTime) => DeletedAt = dateTime;

    }
}
