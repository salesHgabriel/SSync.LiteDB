namespace SSync.Server.LitebDB.Abstractions
{
    public abstract class ISchema
    {
        protected ISchema(Guid id) : base()
        {
            Id = id;
        }

        public Guid Id { get; }

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public long? DeletedAt { get; set; }
    }
}