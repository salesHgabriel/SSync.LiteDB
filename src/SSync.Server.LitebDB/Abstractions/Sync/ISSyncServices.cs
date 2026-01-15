using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions.Sync
{
    public interface ISSyncServices
    {
        IServiceProvider ServiceProvider { get; }

        IInternalISSyncPushRequest<TRequestSchema> PushRequestHandler<TRequestSchema>()
            where TRequestSchema : ISchema;

        ISSyncPullRequest<TRequestCollectionSchema, TParameter> PullRequestHandler<TRequestCollectionSchema, TParameter>()
            where TRequestCollectionSchema : ISchema
            where TParameter : SSyncParameter;
    }
}



// internal class SSyncServices : ISSyncServices
// {
//     public IServiceProvider ServiceProvider { get; }
//     public IInternalISSyncPushRequest<TRequestSchema> PushRequestHandler<TRequestSchema>() where TRequestSchema : ISchema
//     {
//         throw new NotImplementedException();
//     }
//
//     public ISSyncPullRequest<TRequestCollectionSchema, TParameter> PullRequestHandler<TRequestCollectionSchema, TParameter>() where TRequestCollectionSchema : ISchema where TParameter : SSyncParameter
//     {
//         throw new NotImplementedException();
//     }
// }