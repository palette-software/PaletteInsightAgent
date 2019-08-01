using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace PaletteInsightAgent
{

    #region PaletteInsight.Conf description

    namespace Configuration
    {
        // Where the default value of a property is different than the default of its type
        // (like 0 for int or "" for string or null for object) we signal our default
        // value through [DefaultValue()] attribute, so that the YML parser won't write
        // the default values during serialization. And this way we are free to change
        // the default values in the future.

        public class DatabaseConfig
        {
            [DefaultValue("workgroup")]
            [YamlMember(Alias = "Database")]
            public string Database { get; set; } = "workgroup";

            [DefaultValue("localhost")]
            [YamlMember(Alias = "Host")]
            public string Host { get; set; } = "localhost";

            [DefaultValue(8060)]
            [YamlMember(Alias = "Port")]
            public int Port { get; set; } = 8060;

            [DefaultValue("readonly")]
            [YamlMember(Alias = "User")]
            public string User { get; set; } = "readonly";

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

            [YamlMember(Alias = "ProcessName")]
            public string ProcessName { get; set; }
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

            [YamlMember(Alias = "HistoryNeeded")]
            public bool HistoryNeeded { get; set; } = false;
        }

        public class Webservice
        {
            [YamlMember(Alias = "Endpoint")]
            public string Endpoint { get; set; }

            [YamlMember(Alias = "UseProxy")]
            public bool UseProxy { get; set; } = false;

            [DefaultValue("")]
            [YamlMember(Alias = "ProxyAddress")]
            public string ProxyAddress { get; set; } = "";

            [DefaultValue("")]
            [YamlMember(Alias = "ProxyUsername")]
            public string ProxyUsername { get; set; }

            [DefaultValue("")]
            [YamlMember(Alias = "ProxyPassword")]
            public string ProxyPassword { get; set; }
        }

        public class PaletteInsightConfiguration
        {
            [DefaultValue(30)]
            [YamlMember(Alias = "PollInterval")]
            public int PollInterval { get; set; } = 30;

            [DefaultValue(15)]
            [YamlMember(Alias = "ThreadInfoPollInterval")]
            public int ThreadInfoPollInterval { get; set; } = 15;

            [DefaultValue(10)]
            [YamlMember(Alias = "UploadInterval")]
            public int UploadInterval { get; set; } = 10; // Every ten seconds

            [DefaultValue(3600)]
            [YamlMember(Alias = "RepoTablesPollInterval")]
            public int RepoTablesPollInterval { get; set; } = 3600; // Hourly

            [DefaultValue(600)]
            [YamlMember(Alias = "StreamingTablesPollInterval")]
            public int StreamingTablesPollInterval { get; set; } = 300; // 5 minutes

            [DefaultValue(true)]
            [YamlMember(Alias = "PreferPassiveRepository")]
            public bool PreferPassiveRepository { get; set; } = true;   // Poll passive Tableau repository if it exists by default

            [YamlMember(Alias = "ProcessedFilesTTL")]
            public int ProcessedFilesTTL { get; set; } = 0; // Default is delete immediately

            [DefaultValue(10 * 1024)]
            [YamlMember(Alias = "StorageLimit")]
            public int StorageLimit { get; set; } = 10 * 1024; // Default is 10 GB. This value is given in megabytes.

            [DefaultValue(300)]
            [YamlMember(Alias = "LogPollInterval")]
            public int LogPollInterval { get; set; } = 300;

            [DefaultValue(true)]
            [YamlMember(Alias = "AllProcesses")]
            public bool AllProcesses { get; set; } = true;

            [YamlMember(Alias = "InsightAuthToken")]
            public string InsightAuthToken { get; set; }

            [YamlMember(Alias = "Webservice")]
            public Webservice Webservice { get; set; }

            [YamlMember(Alias = "Logs")]
            public List<LogFolder> Logs { get; set; }

            [YamlMember(Alias = "TableauRepo")]
            public DatabaseConfig TableauRepo { get; set; }

            // Log polling options

            [DefaultValue(true)]
            [YamlMember(Alias = "UseCounterSamples")]
            public bool UseCounterSamples { get; set; } = true;

            [DefaultValue(true)]
            [YamlMember(Alias = "UseThreadInfo")]
            public bool UseThreadInfo { get; set; } = true;

            [DefaultValue(true)]
            [YamlMember(Alias = "UseLogPolling")]
            public bool UseLogPolling { get; set; } = true;

            [DefaultValue(true)]
            [YamlMember(Alias = "UseRepoPolling")]
            public bool UseRepoPolling { get; set; } = true;

            /// <summary>
            /// The maximum lines to be parsed in a batch from a log file
            /// </summary>
            [DefaultValue(10000)]
            [YamlMember(Alias = "LogLinesPerBatch")]
            public int LogLinesPerBatch { get; set; } = 10000;

            /// <summary>
            /// The maximum records to be read in a batch from a streaming table
            /// </summary>
            [DefaultValue(100000)]
            [YamlMember(Alias = "StreamingTablesPollLimit")]
            public int StreamingTablesPollLimit { get; set; } = 100000;
        }

        #endregion
    }

}
