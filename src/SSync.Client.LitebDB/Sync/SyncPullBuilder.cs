using LiteDB;
using SSync.Client.LitebDB.Abstractions;
using SSync.Client.LitebDB.Abstractions.Sync;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSync.Client.LitebDB.Sync
{
    public class SyncPullBuilder : IBuilder
    {
        private readonly List<Func<object>> _actions = new List<Func<object>>();
        public List<object> DatabaseLocalChanges { get; } = new List<object>();
        public string? JsonDatabaseLocalChanges { get; private set; }

        public SyncPullBuilder AddPullSync<T>(Func<SchemaPullResult<T>> action) where T : SchemaSync
        {
            _actions.Add(() => action());
            return this;
        }

        public void Build()
        {
            DatabaseLocalChanges.Clear();
            foreach (var act in _actions)
            {
                DatabaseLocalChanges.Add(act());
            }

            _actions.Clear();

            SetChangesToJson();
        }

        public string? GetChangesToJson(JsonSerializerOptions? opt = null)
        {
            return DatabaseLocalChanges is not null && DatabaseLocalChanges.Count != 0
                ? System.Text.Json.JsonSerializer.Serialize(DatabaseLocalChanges, opt)
                : string.Empty;
        }

        public void SetChangesToJson()
        {
            JsonDatabaseLocalChanges = System.Text.Json.JsonSerializer.Serialize(DatabaseLocalChanges);
        }

        public DateTime GetTimestampFromJson()
        {
            if (string.IsNullOrEmpty(JsonDatabaseLocalChanges))
                return DateTime.MinValue;

            var jsonArray = JsonArray.Parse(JsonDatabaseLocalChanges)!.AsArray();

            var timeStamp = jsonArray.Any() ? jsonArray.First()!["timestamp"] ?? jsonArray.First()!["Timestamp"] : DateTime.MinValue;

            var dta = System.Text.Json.JsonSerializer.Deserialize<DateTimeOffset>(timeStamp.ToJsonString()).Date;

            return dta;
        }
    }
}