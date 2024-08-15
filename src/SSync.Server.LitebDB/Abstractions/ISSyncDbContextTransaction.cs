namespace SSync.Server.LitebDB.Abstractions
{
    public interface ISSyncDbContextTransaction
    {
        Task CommitSyncAsync();

        Task BeginTransactionSyncAsync();

        Task CommitTransactionSyncAsync();

        Task RollbackTransactionSyncAsync();
    }
}
