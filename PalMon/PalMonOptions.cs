using System;
using System.Collections.Generic;
using PalMon.Helpers;
using PalMon.Output;

namespace PalMon
{
    /// <summary>
    /// Encapsulates runtime options for PalMonAgent.
    /// </summary>
    public class PalMonOptions
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
        public int ThreadInfoPollInterval { get; set; }
        private static PalMonOptions instance;
        private const int MinPollInterval = 1; // In seconds.
        public ICollection<string> Processes { get; set; }

        public ICollection<LogFolderInfo> LogFolders { get; set; }

        #region Repo properties

        public string RepoHost { get; set; }
        public Int32 RepoPort { get; set; }
        public string RepoUser { get; set; }
        public string RepoPass { get; set; }
        public string RepoDb { get; set; }

        #endregion Repo properties

        #region Singleton Constructor/Accessor

        private PalMonOptions()
        {
            LogFolders = new List<LogFolderInfo>();
        }

        public static PalMonOptions Instance
        {
            get { return instance ?? (instance = new PalMonOptions()); }
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
                && ThreadInfoPollInterval >= MinPollInterval;
        }

        public override string ToString()
        {
            return String.Format("[PollInterval='{1}', LogPollInterval='{2}', ThreadInfoPollInterval='{3}']",
                                   PollInterval, LogPollInterval, ThreadInfoPollInterval);
        }

        #endregion
    }
}