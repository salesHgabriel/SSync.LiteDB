using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public interface ISSyncPullRequest<TSchema, in TParameter>
    where TSchema : ISchema
    where TParameter : SSyncParameter
    {
        Task<IEnumerable<TSchema>> QueryAsync(TParameter parameter);
    }
}