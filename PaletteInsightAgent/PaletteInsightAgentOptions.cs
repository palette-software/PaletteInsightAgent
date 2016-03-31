using System;
using System.Collections.Generic;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Output;
using PaletteInsight.Configuration;
using PaletteInsightAgent.Output.OutputDrivers;

namespace PaletteInsightAgent
{
    /// <summary>
    /// Encapsulates runtime options for PaletteInsightAgent.
    /// </summary>
    public class PaletteInsightAgentOptions
    {
        public struct LogFolderInfo
        {
            public string FolderToWatch { get; set; }
            public string DirectoryFilter { get; set; }
        }
        [CLSCompliant(false)]
        public IDbConnectionInfo ResultDatabase { get; set; }
        public int PollInterval { get; set; }
        public int LogPollInterval { get; set; }
        public int RepoTablesPollInterval { get; set; }
        public int ThreadInfoPollInterval { get; set; }
        public int DBWriteInterval { get; set; }
        public int ProcessedFilestTTL { get; set; }
        public long StorageLimit { get; set; }
        public bool AllProcesses { get; set; }
        private static PaletteInsightAgentOptions instance;
        private const int MinPollInterval = 1; // In seconds.
        public IDictionary<string, ProcessData> Processes { get; set; }
        public ICollection<RepoTable> RepositoryTables { get; set; }

        public ICollection<LogFolderInfo> LogFolders { get; set; }

        public WebserviceConfiguration WebserviceConfig { get; set; }

        // Log polling options

        public bool UseCounterSamples { get; set; }
        public bool UseThreadInfo { get; set; }
        public bool UseLogPolling { get; set; }
        public bool UseRepoPolling { get; set; }

        public bool UseHeartbeat { get; set; }

        #region Repo properties

        public IDbConnectionInfo RepositoryDatabase { get; set; }

        #endregion Repo properties

        #region Singleton Constructor/Accessor

        private PaletteInsightAgentOptions()
        {
            LogFolders = new HashSet<LogFolderInfo>();
        }

        public static PaletteInsightAgentOptions Instance
        {
            get { return instance ?? (instance = new PaletteInsightAgentOptions()); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indicates whether the current options configuration is valid or not.
        /// </summary>
        /// <returns>True if current options are all valid.</returns>
        public bool Valid()
        {
            // removed the host from here as we arent using that functionality
            return PollInterval >= MinPollInterval
                && LogPollInterval >= MinPollInterval
                && ThreadInfoPollInterval >= MinPollInterval
                && DBWriteInterval >= MinPollInterval;
        }

        public override string ToString()
        {
            return String.Format("[PollInterval='{0}', LogPollInterval='{1}', ThreadInfoPollInterval='{2}'], DBWriteInterval='{3}'",
                                   PollInterval, LogPollInterval, ThreadInfoPollInterval, DBWriteInterval);
        }

        #endregion
    }
}