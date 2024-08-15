namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public interface ISSyncPushRequest<TSchema> where TSchema : ISchema
    {
        Task<TSchema?> FindByIdAsync(Guid id);

        Task<bool> CreateAsync(TSchema schema);

        Task<bool> UpdateAsync(TSchema schema);

        Task<bool> DeleteAsync(TSchema schema);
    }
}