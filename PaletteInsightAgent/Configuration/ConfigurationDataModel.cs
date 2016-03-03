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

        public class Webservice
        {
            [YamlMember(Alias = "Endpoint")]
            public string Endpoint { get; set; }
        }

        public class PaletteInsightConfiguration
        {
            [YamlMember(Alias = "PollInterval")]
            public int PollInterval { get; set; }

            [YamlMember(Alias = "ThreadInfoPollInterval")]
            public int ThreadInfoPollInterval { get; set; }

            [YamlMember(Alias = "RepoTablesPollInterval")]
            public int RepoTablesPollInterval { get; set; } = 3600; // Hourly

            [YamlMember(Alias = "ProcessedFilesTTL")]
            public int ProcessedFilesTTL { get; set; } = 604800; // Default is a week

            [YamlMember(Alias = "LogPollInterval")]
            public int LogPollInterval { get; set; }

            [YamlMember(Alias = "DBWriteInterval")]
            public int DBWriteInterval { get; set; }

            [YamlMember(Alias = "AllProcesses")]
            public bool AllProcesses { get; set; } = false;

            [YamlMember(Alias = "Database")]
            public DatabaseConfig Database { get; set; }

            [YamlMember(Alias = "Webservice")]
            public Webservice Webservice { get; set; }

            [YamlMember(Alias = "Logs")]
            public List<LogFolder> Logs { get; set; }

            [YamlMember(Alias = "TableauRepo")]
            public DatabaseConfig TableauRepo { get; set; }
        }

        #endregion
    }

}
