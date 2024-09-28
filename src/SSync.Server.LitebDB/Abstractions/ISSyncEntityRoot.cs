namespace SSync.Server.LitebDB.Abstractions
{
    public interface ISSyncEntityRoot
    {
        public Guid Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
