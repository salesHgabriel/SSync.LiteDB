using SSync.Server.LitebDB.Engine;
using System.Text.Json.Nodes;

namespace SSync.Server.LitebDB.Abstractions
{
    public interface ISchemaCollection
    {
        Task<List<object>> PullChangesAsync(SSyncParameter parameter, SSyncOptions? options = null);
        IAsyncEnumerable<object> PullStreamChanges(SSyncParameter parameter, SSyncOptions? options = null);
        Task<DateTime> PushChangesAsync(JsonArray changes, SSyncParameter parameter, SSyncOptions? options = null);
    }
}