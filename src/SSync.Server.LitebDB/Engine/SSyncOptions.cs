using SSync.Server.LitebDB.Abstractions;

namespace SSync.Server.LitebDB.Engine
{
    public class SSyncOptions
    {
        public Time TimeConfig { get; set; } = Time.UTC;
    }
}
