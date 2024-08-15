using SSync.Server.LitebDB.Abstractions.Builders;
using SSync.Server.LitebDB.Abstractions;


namespace SSync.Server.LitebDB.Engine.Builders
{
    public class PushExecutionOrderBuilder : IPushExecutionOrderStep
    {
        private readonly Dictionary<string, Type> _schemas = new Dictionary<string, Type>();
        private readonly List<string> _orderedCollections = new List<string>();

        public IPushExecutionOrderStep ThenBy<T>(string collectionName) where T : ISchema
        {
            _orderedCollections.Add(collectionName);
            _schemas[collectionName] = typeof(T);
            return this;
        }

        public IPushExecutionOrderStep By<T>(string collectionName) where T : ISchema
        {
            _orderedCollections.Add(collectionName);
            _schemas[collectionName] = typeof(T);
            return this;
        }


        public List<string> GetCollectionOrder()
        {
            return _orderedCollections;
        }

        public Dictionary<string, Type> GetSchemas()
        {
            return _schemas;
        }
    }
}
