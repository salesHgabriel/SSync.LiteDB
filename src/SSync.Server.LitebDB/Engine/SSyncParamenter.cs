using System.Text.Json.Serialization;

namespace SSync.Server.LitebDB.Engine
{
    public class SSyncParamenter
    {
        public string[] Colletions { get; set; } = [];
        public long Timestamp { get; set; } = 0;

        [JsonIgnore]
        internal string CurrentColletion { get; set; } = string.Empty;

    }
}
