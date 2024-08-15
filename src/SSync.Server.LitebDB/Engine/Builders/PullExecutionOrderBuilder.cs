using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions.Sync;

namespace SSync.Server.LitebDB.Engine.Builders
{
    public class PullExecutionOrderBuilder : IPullExecutionOrderStep
    {
        private readonly List<(Type SyncType, string Parameter)> _steps = [];

        public IPullExecutionOrderStep By<TSync>(string collection) where TSync : ISchema
        {
            _steps.Add((typeof(TSync), collection));
            return this;
        }

        public IPullExecutionOrderStep ThenBy<TSync>(string collection) where TSync : ISchema
        {
            _steps.Add((typeof(TSync), collection));
            return this;
        }

        public List<(Type SyncType, string Parameter)> GetSteps() => _steps;

        public async Task ExecuteAsync<TParameter>(TParameter parameter, string[] filter)
            where TParameter : SSyncParamenter
        {
            var steps = GetSteps();
            foreach (var (SyncType, Parameter) in steps)
            {
                if (filter.Contains(Parameter))
                {
                    var handlerType = typeof(ISSyncPullRequest<,>).MakeGenericType(SyncType, typeof(TParameter));
                    //var handler = Activator.CreateInstance(handlerType) as ISSyncPullRequest<ISchema, TParameter>;
                    if (Activator.CreateInstance(handlerType) is ISSyncPullRequest<ISchema, TParameter> handler)
                    {
                        _ = await handler.QueryAsync(parameter);
                    }
                }
            }
        }
    }
}