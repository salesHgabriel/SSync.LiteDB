using LiteDB;
using SSync.Client.LitebDB.Poco;

namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public interface ISynchronize
    {
        DateTime GetLastPulledAt();
        BsonValue ReplaceLastPulledAt(DateTime lastPulledAt);

        BsonValue DeleteSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;

        BsonValue DeleteSync<T>(T entity, string? colName) where T : SchemaSync;

        void DumpLogOutput(string title = "log.txt");

        T FindByIdSync<T>(Guid id, ILiteCollection<T> col) where T : SchemaSync;

        T FindByIdSync<T>(Guid id, string? colName) where T : SchemaSync;
        BsonValue InsertSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;

        BsonValue InsertSync<T>(T entity, string? colName) where T : SchemaSync;

        SchemaPullResult<T> PullChangesResult<T>(DateTime lastPulledAt, string collectionName) where T : SchemaSync;

        SchemaPush<T> PushChangesResult<T>(SchemaPush<T> schemaPush) where T : SchemaSync;
        BsonValue UpdateSync<T>(T entity, string? colName) where T : SchemaSync;

        BsonValue UpdateSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;
    }
}