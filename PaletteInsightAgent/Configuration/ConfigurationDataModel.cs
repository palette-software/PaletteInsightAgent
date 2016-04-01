using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace PaletteInsight
{

    #region PaletteInsight.Conf description

    namespace Configuration
    {
        public class DatabaseConfig
        {
            [YamlMember(Alias = "Database")]
            public string Database { get; set; }

            [YamlMember(Alias = "Host")]
            public string Host { get; set; }

            [YamlMember(Alias = "Port")]
            public int Port { get; set; }

            [YamlMember(Alias = "User")]
            public string User { get; set; }

            [YamlMember(Alias = "Password")]
            public string Password { get; set; }

            [YamlMember(Alias = "CommandTimeout")]
            public int CommandTimeout { get; set; }
        }

        public class LogFolder
        {
            [YamlMember(Alias = "Directory")]
            public string Directory { get; set; }

            [YamlMember(Alias = "Filter")]
            public string Filter { get; set; }
        }

        public class ProcessData
        {
            [YamlMember(Alias = "Name")]
            public string Name { get; set; }

            [YamlMember(Alias = "Granularity")]
            public string Granularity { get; set; } = "Process";
        }

        public class RepoTable
        {
            [YamlMember(Alias = "Name")]
            public string Name { get; set; }

            [YamlMember(Alias = "Full")]
            public bool Full { get; set; } = true;

            [YamlMember(Alias = "Field")]
            public string Field { get; set; }

            [YamlMember(Alias = "Filter")]
            public string Filter { get; set; }
        }

        public class Webservice
        {
            [YamlMember(Alias = "Endpoint")]
            public string Endpoint { get; set; }

            [YamlMember(Alias = "UseProxy")]
            public bool UseProxy { get; set; } = false;

            [YamlMember(Alias = "ProxyAddress")]
            public string ProxyAddress { get; set; } = "";

            [YamlMember(Alias = "ProxyUsername")]
            public string ProxyUsername { get; set; } = "";

            [YamlMember(Alias = "ProxyPassword")]
            public string ProxyPassword { get; set; } = "";
        }

        public class PaletteInsightConfiguration
        {
            // Deprecated. Referring section needs to be removed from all config files first.
            [YamlMember(Alias = "Database")]
            public DatabaseConfig Database { get; set; }
 

            [YamlMember(Alias = "PollInterval")]
            public int PollInterval { get; set; }

            [YamlMember(Alias = "ThreadInfoPollInterval")]
            public int ThreadInfoPollInterval { get; set; }

            [YamlMember(Alias = "RepoTablesPollInterval")]
            public int RepoTablesPollInterval { get; set; } = 3600; // Hourly

            [YamlMember(Alias = "ProcessedFilesTTL")]
            // public int ProcessedFilesTTL { get; set; } = 604800; // Default is a week
            public int ProcessedFilesTTL { get; set; } = 0; // Default is delete immediately

            [YamlMember(Alias = "StorageLimit")]
            public int StorageLimit { get; set; } = 1 * 1024; // Default is 1 Gb. This value is given in megabytes.

            [YamlMember(Alias = "LogPollInterval")]
            public int LogPollInterval { get; set; }

            [YamlMember(Alias = "DBWriteInterval")]
            public int DBWriteInterval { get; set; }

            // Became outdated as it was misconfigured at clients
            [YamlMember(Alias = "AllProcesses")]
            public bool AllProcesses { get; set; } = true;

            // This is a hack and as such I chose very ugly name intentionally so that we don't leave it this way.
            [YamlMember(Alias = "AllProcesses2")]
            public bool AllProcesses2 { get; set; } = true;

            [YamlMember(Alias = "Webservice")]
            public Webservice Webservice { get; set; }

            [YamlMember(Alias = "Logs")]
            public List<LogFolder> Logs { get; set; }

            [YamlMember(Alias = "TableauRepo")]
            public DatabaseConfig TableauRepo { get; set; }

            // Log polling options

            [YamlMember(Alias = "UseCounterSamples")]
            public bool UseCounterSamples { get; set; } = true;

            [YamlMember(Alias = "UseThreadInfo")]
            public bool UseThreadInfo { get; set; } = true;

            [YamlMember(Alias = "UseLogPolling")]
            public bool UseLogPolling { get; set; } = true;

            [YamlMember(Alias = "UseRepoPolling")]
            public bool UseRepoPolling { get; set; } = true;

            /// <summary>
            /// The maximum lines to be parsed in a batch from a log file
            /// </summary>
            [YamlMember(Alias = "LogLinesPerBatch")]
            public int LogLinesPerBatch { get; set; } = 10000;

        }

        #endregion
    }

}
