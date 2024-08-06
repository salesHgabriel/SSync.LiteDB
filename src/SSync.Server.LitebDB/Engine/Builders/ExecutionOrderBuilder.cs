using SSync.Server.LitebDB.Abstractions;
using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions.Sync;


namespace SSync.Server.LitebDB.Engine.Builders
{
    public class ExecutionOrderBuilder : IExecutionOrderStep
    {
        private readonly List<(Type SyncType, string Parameter)> _steps = new();

        public IExecutionOrderStep By<TSync>(string collection) where TSync : ISchema
        {
            _steps.Add((typeof(TSync), collection));
            return this;
        }

        public IExecutionOrderStep ThenBy<TSync>(string collection) where TSync : ISchema
        {
            _steps.Add((typeof(TSync), collection));
            return this;
        }

        public List<(Type SyncType, string Parameter)> GetSteps() => _steps;


        public async Task ExecuteAsync<TParameter>(TParameter parameter, string[] filter)
            where TParameter : SSyncParamenter
        {
            var steps = GetSteps();
            foreach (var step in steps)
            {
                if (filter.Contains(step.Parameter))
                {
                    var handlerType = typeof(ISSyncPullRequest<,>).MakeGenericType(step.SyncType, typeof(TParameter));
                    var handler = Activator.CreateInstance(handlerType) as ISSyncPullRequest<ISchema, TParameter>;
                    if (handler != null)
                    {
                        var task = await handler.QueryAsync(parameter);
                    }
                }
            }
        }
    }


}
