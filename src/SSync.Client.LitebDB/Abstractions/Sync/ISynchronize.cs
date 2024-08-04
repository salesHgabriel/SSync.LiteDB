
using LiteDB;
using SSync.Client.LitebDB.Poco;

namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public interface ISynchronize
    {
        BsonValue DeleteSync<T>(T entity, ILiteCollection<T> col) where T : BaseSync;
        BsonValue DeleteSync<T>(T entity, string? colName) where T : BaseSync;
        void DumpLogOutput(string title = "log.txt", ConsoleColor consoleColor = ConsoleColor.Yellow);
        T FindByIdSync<T>(Guid id, ILiteCollection<T> col) where T : BaseSync;
        T FindByIdSync<T>(Guid id, string? colName) where T : BaseSync;
        BsonValue InsertSync<T>(T entity, ILiteCollection<T> col) where T : BaseSync;
        BsonValue InsertSync<T>(T entity, string? colName) where T : BaseSync;
        SchemaPullResult<T> PullChangesResult<T>(long lastPulledAt, string collectionName, DateTime now) where T : BaseSync;
        SchemaPush<T> PushChangesResult<T>(SchemaPush<T> schemaPush) where T : BaseSync;
        BsonValue UpdateSync<T>(T entity, string? colName) where T : BaseSync;
        BsonValue UpdateSync<T>(T entity, ILiteCollection<T> col) where T : BaseSync;
    }
}
