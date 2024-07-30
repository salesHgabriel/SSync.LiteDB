namespace SSync.Client.LitebDB.Abstractions
{
    public class SynchronizeOptions
    {
        /// <summary>
        /// This enum to view operation like logs
        /// </summary>
        public Mode Mode { get; set; } = Mode.RELEASE;

        /// <summary>
        /// Path in save database
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// This save file on path ConnectionString in .txt
        /// If not set true show output terminal
        /// </summary>
        public bool SaveLogOnFile { get; set; } = false;
    }
}
