using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions.Builders
{
    public interface IPullExecutionOrderStep
    {
        IPullExecutionOrderStep By<TSync>(string collection) where TSync : ISchema;

        IPullExecutionOrderStep ThenBy<TSync>(string collection) where TSync : ISchema;

        Task ExecuteAsync<TParameter>(TParameter parameter, string[] filter) where TParameter : SSyncParamenter;

        List<(Type SyncType, string Parameter)> GetSteps();
    }
}