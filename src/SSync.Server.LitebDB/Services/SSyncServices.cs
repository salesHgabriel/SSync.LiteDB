using Microsoft.Extensions.DependencyInjection;
using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Sync;
using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Services
{
    internal class SSyncServices : ISSyncServices
    {
        public SSyncServices(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public ISSyncPullRequest<TRequestSchema, TParameter> PullRequestHandler<TRequestSchema, TParameter>()
            where TRequestSchema : ISchema
            where TParameter : SSyncParamenter
                => ServiceProvider.GetRequiredService<ISSyncPullRequest<TRequestSchema, TParameter>>();

        public IInternalISSyncPushRequest<TRequestSchema> PushRequestHandler<TRequestSchema>() where TRequestSchema : ISchema
            => ServiceProvider.GetRequiredService<IInternalISSyncPushRequest<TRequestSchema>>();
    }
}