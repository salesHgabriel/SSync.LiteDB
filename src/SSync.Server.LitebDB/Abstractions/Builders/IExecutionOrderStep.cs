using SSync.Server.LitebDB.Engine;

namespace SSync.Server.LitebDB.Abstractions.Builders
{
    public interface IExecutionOrderStep
    {
        IExecutionOrderStep By<TSync>(string collection) where TSync : ISchema;
        IExecutionOrderStep ThenBy<TSync>(string collection) where TSync : ISchema;
        Task ExecuteAsync<TParameter>(TParameter parameter, string[] filter) where TParameter : SSyncParamenter;
    }
}
