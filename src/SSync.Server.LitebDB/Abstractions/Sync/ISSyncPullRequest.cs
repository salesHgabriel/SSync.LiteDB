using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public interface ISSyncPullRequest<TSchema, in TParameter>
    where TSchema : ISchema
    where TParameter : SSyncParamenter
    {
        Task<IEnumerable<TSchema>> Query(TParameter parameter);
    }
}
