using System.Text.Json.Serialization;

namespace SSync.Server.LitebDB.Engine
{
    public class SSyncParameter
    {
        public string[] Colletions { get; set; } = [];
        public DateTime Timestamp { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        internal string CurrentColletion { get; set; } = string.Empty;
    }
}