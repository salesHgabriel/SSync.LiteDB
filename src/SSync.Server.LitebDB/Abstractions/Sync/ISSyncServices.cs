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
            where TParameter : SSyncParamenter;
    }
}