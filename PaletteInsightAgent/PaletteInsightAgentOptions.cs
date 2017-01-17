using System;
using System.Collections.Generic;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Output;
using PaletteInsight.Configuration;
using PaletteInsightAgent.Output.OutputDrivers;
using System.IO;

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
            public string LogFormat { get; set; }

            public static LogFolderInfo Create(string path, string filter, string logFormat)
            {
                // get a full path without the end separators
                var fullPath = Path.GetFullPath(path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToLower();

                // Lowercase the paths so different cased paths do still conflict
                return new LogFolderInfo
                {
                    FolderToWatch = fullPath.ToLower(),
                    DirectoryFilter = filter.ToLower(),
                    LogFormat = logFormat.ToLower(),
                };
            }

            /// <summary>
            /// Returns a unique string for this LogFolderInfo which matches the ToValueString()
            /// return value of any other LogFolderInfo with the same values.
            /// </summary>
            /// <returns></returns>
            public string ToValueString()
            {
                return String.Format("{0}|||{1}|||{2}", FolderToWatch, DirectoryFilter, LogFormat);
            }
        }
        public int PollInterval { get; set; }
        public int LogPollInterval { get; set; }
        public int UploadInterval { get; set; }
        public int RepoTablesPollInterval { get; set; }
        public int StreamingTablesPollInterval { get; set; }
        public int ThreadInfoPollInterval { get; set; }
        public int ProcessedFilestTTL { get; set; }
        public long StorageLimit { get; set; }
        public bool AllProcesses { get; set; }
        private static PaletteInsightAgentOptions instance;
        private const int MinPollInterval = 1; // In seconds.
        public IDictionary<string, ProcessData> Processes { get; set; }
        public ICollection<RepoTable> RepositoryTables { get; set; }

        public ICollection<LogFolderInfo> LogFolders { get; set; }

        public string AuthToken { get; set; }

        public WebserviceConfiguration WebserviceConfig { get; set; }

        // Log polling options

        public bool UseCounterSamples { get; set; }
        public bool UseThreadInfo { get; set; }
        public bool UseLogPolling { get; set; }
        public bool UseRepoPolling { get; set; }
        public bool UseStreamingTables { get; set; }

        public int LogLinesPerBatch { get; set; }

        #region Repo properties

        public DbConnectionInfo RepositoryDatabase { get; set; }

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
                && UploadInterval >= MinPollInterval;
        }

        public override string ToString()
        {
            return String.Format("[PollInterval='{0}', LogPollInterval='{1}', ThreadInfoPollInterval='{2}', UploadInterval='{3}'] ",
                                   PollInterval, LogPollInterval, ThreadInfoPollInterval, UploadInterval);
        }

        #endregion
    }
}