﻿using System.Collections.Generic;
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

            [YamlMember(Alias = "Format")]
            //public string Format { get; set; } = "json";
            // Until the Talend side is not updated, the default format name should remain "server"
            public string Format { get; set; } = "server";
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
            // Deprecated. Referring section needs to be removed from all config files first.
            [YamlMember(Alias = "DBWriteInterval")]
            public int DBWriteInterval { get; set; } = 0;

            [YamlMember(Alias = "PollInterval")]
            public int PollInterval { get; set; } = 30;

            [YamlMember(Alias = "ThreadInfoPollInterval")]
            public int ThreadInfoPollInterval { get; set; } = 15;

            [YamlMember(Alias = "UploadInterval")]
            public int UploadInterval { get; set; } = 10; // Every ten seconds

            [YamlMember(Alias = "RepoTablesPollInterval")]
            public int RepoTablesPollInterval { get; set; } = 3600; // Hourly

            [YamlMember(Alias = "StreamingTablesPollInterval")]
            public int StreamingTablesPollInterval { get; set; } = 600; // 10 minutes

            [YamlMember(Alias = "ProcessedFilesTTL")]
            // public int ProcessedFilesTTL { get; set; } = 604800; // Default is a week
            public int ProcessedFilesTTL { get; set; } = 0; // Default is delete immediately

            [YamlMember(Alias = "StorageLimit")]
            public int StorageLimit { get; set; } = 10 * 1024; // Default is 10 GB. This value is given in megabytes.

            [YamlMember(Alias = "LogPollInterval")]
            public int LogPollInterval { get; set; } = 300;

            [YamlMember(Alias = "AllProcesses")]
            public bool AllProcesses { get; set; } = true;

            [YamlMember(Alias = "LicenseKey")]
            public string LicenseKey { get; set; }

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

            [YamlMember(Alias = "IsPrimaryNode")]
            public bool IsPrimaryNode { get; set; } = false;

            /// <summary>
            /// The maximum lines to be parsed in a batch from a log file
            /// </summary>
            [YamlMember(Alias = "LogLinesPerBatch")]
            public int LogLinesPerBatch { get; set; } = 10000;

        }

        #endregion
    }

}
