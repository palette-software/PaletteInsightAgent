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
        [CLSCompliant(false)]
        public IDbConnectionInfo ResultDatabase { get; set; }
        public ICollection<Host> Hosts { get; set; }
        public int PollInterval { get; set; }
        public int LogPollInterval { get; set; }
        public int ThreadInfoPollInterval { get; set; }
        public string TableName { get; set; }
        private static PalMonOptions instance;
        private const int MinPollInterval = 1; // In seconds.
        public ICollection<string> Processes { get; set; }

        public string DatabaseType { get; set; }

        #region LogPoller config settings

        public string FolderToWatch { get; set; }
        public string DirectoryFilter { get; set; }

        #endregion LogPoller config settings

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
            Hosts = new List<Host>();
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
            return Hosts.Count > 0
                && PollInterval >= MinPollInterval
                && LogPollInterval >= MinPollInterval
                && ThreadInfoPollInterval >= MinPollInterval;
        }

        public override string ToString()
        {
            return String.Format("[{0}='{1}', PollInterval='{2}', LogPollInterval='{3}', ThreadInfoPollInterval='{4}', TableName='{5}']",
                                   "Host".Pluralize(Hosts.Count), String.Join(",", Hosts), PollInterval, LogPollInterval, ThreadInfoPollInterval, TableName);
        }

        #endregion
    }
}