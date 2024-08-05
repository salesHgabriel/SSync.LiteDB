
namespace SSync.Server.LitebDB.Abstractions
{
    public abstract class ISchema
    {
        protected ISchema(Guid id) : base()
        {
            Id = id;
        }
        public Guid Id { get;  }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
