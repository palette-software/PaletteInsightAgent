using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace PaletteInsight
{

    #region PaletteInsight.Conf description

    namespace Configuration
    {
        public class Database
        {
            [YamlMember(Alias = "Database")]
            public string Name { get; set; }

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

        public class PaletteInsightConfiguration
        {
            [YamlMember(Alias = "PollInterval")]
            public int PollInterval { get; set; }

            [YamlMember(Alias = "ThreadInfoPollInterval")]
            public int ThreadInfoPollInterval { get; set; }

            [YamlMember(Alias = "LogPollInterval")]
            public int LogPollInterval { get; set; }

            [YamlMember(Alias = "Processes")]
            public List<string> Processes { get; set; }

            [YamlMember(Alias = "Database")]
            public Database Database { get; set; }

            [YamlMember(Alias = "Logs")]
            public List<LogFolder> Logs { get; set; }

            [YamlMember(Alias = "TableauRepo")]
            public Database TableauRepo { get; set; }
        }

        #endregion
    }

}
