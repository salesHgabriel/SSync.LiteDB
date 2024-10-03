using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Abstractions
{
    public abstract class ISSyncEntityRoot
    {
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

        void SetUpdatedAt(DateTime dateTime) => UpdatedAt = dateTime;
        void SetDeletedAt(DateTime dateTime) => DeletedAt = dateTime;

    }
}
