namespace SSync.Server.LitebDB.Abstractions.Builders
{
    public interface IPushExecutionOrderStep
    {
        IPushExecutionOrderStep By<TSync>(string collection) where TSync : ISchema;

        List<string> GetCollectionOrder();

        Dictionary<string, Type> GetSchemas();

        IPushExecutionOrderStep ThenBy<TSync>(string collection) where TSync : ISchema;
    }
}