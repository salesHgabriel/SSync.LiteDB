
using SQLite;
using SSync.Client.SQLite.Poco;

namespace SSync.Client.SQLite.Abstractions.Sync
{
    public interface ISynchronize
    {
        //DateTime GetLastPulledAt();
        //BsonValue ReplaceLastPulledAt(DateTime lastPulledAt);

        //BsonValue DeleteSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;

        //BsonValue DeleteSync<T>(T entity, string? colName) where T : SchemaSync;

        //void DumpLogOutput(string title = "log.txt");

        //T FindByIdSync<T>(Guid id, ILiteCollection<T> col) where T : SchemaSync;

        //T FindByIdSync<T>(Guid id, string? colName) where T : SchemaSync;
        //BsonValue InsertSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;

        //BsonValue InsertSync<T>(T entity, string? colName) where T : SchemaSync;

        //SchemaPullResult<T> PullChangesResult<T>(DateTime lastPulledAt, string collectionName) where T : SchemaSync;

        //SchemaPush<T> PushChangesResult<T>(SchemaPush<T> schemaPush) where T : SchemaSync;
        //BsonValue UpdateSync<T>(T entity, string? colName) where T : SchemaSync;

        //BsonValue UpdateSync<T>(T entity, ILiteCollection<T> col) where T : SchemaSync;
        Task<int> DeleteSyncAsync<T>(T entity) where T : SchemaSync, new();
        void Dispose();
        void DumpLogOutput(string title = "log.txt");
        Task<T> FindByIdSyncAsync<T>(Guid id) where T : SchemaSync, new();
        Task<DateTime> GetLastPulledAtAsync();
        Task<int> InsertSyncAsync<T>(T entity) where T : SchemaSync, new();
        Task<SchemaPullResult<T>> PullChangesResultAsync<T>(DateTime lastPulledAt, string collectionName) where T : SchemaSync, new();
        Task<SchemaPush<T>> PushChangesResultAsync<T>(SchemaPush<T> schemaPush) where T : SchemaSync, new();
        Task<int> ReplaceLastPulledAtAsync(DateTime lastPulledAt);
        Task<int> UpdateSyncAsync<T>(T entity) where T : SchemaSync, new();
    }
}