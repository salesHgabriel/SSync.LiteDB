﻿using SSync.Shared.ClientServer.LitebDB.Enums;

namespace SSync.Client.LitebDB.Abstractions.Sync
{
    public class SynchronizeOptions
    {
        /// <summary>
        /// This enum to view operation like logs
        /// </summary>
        public Mode Mode { get; set; } = Mode.RELEASE;

        /// <summary>
        /// Path in save file text to debug log
        /// </summary>
        public string PathFile { get; set; } = string.Empty;

        /// <summary>
        /// This save file on path in .txt
        /// If not set true will show output on terminal
        /// </summary>
        public bool SaveLogOnFile { get; set; } = false;
    }
}
