
using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public interface IInternalISSyncPushRequest<TSchema> where TSchema : ISchema
    {
        Task<bool> UpsertAsync(TSchema schema, DateTime lastPulledAt, Time? time = Time.UTC);

        Task<bool> DeleteAsync(Guid key, DateTime lastPulledAt, Time? time = Time.UTC);
    }
}