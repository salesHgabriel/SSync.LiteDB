
using SSync.Server.LitebDB.Enums;

namespace SSync.Server.LitebDB.Engine
{
    public class SSyncOptions
    {
        /// <summary>
        /// Configure your timestamp
        /// IMPORTANT: Required same your client
        /// </summary>
        public Time TimeConfig { get; set; } = Time.UTC;

        /// <summary>
        /// Path in save file text to debug log
        /// </summary>
        public string PathFile { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// This save file on path in .txt
        /// If not set true will show output on terminal
        /// </summary>
        public bool SaveLogOnFile { get; set; } = false;

        /// <summary>
        /// This enum to view operation like logs
        /// </summary>
        public Mode Mode { get; set; } = Mode.RELEASE;
    }
}