
using SSync.Client.SQLite.Poco;

namespace SSync.Client.SQLite.Abstractions.Sync
{
    public interface ISynchronize
    {
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