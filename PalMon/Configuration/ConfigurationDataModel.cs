using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaletteInsight
{

    #region PaletteInsight.Conf description

    namespace Configuration
    {

        public class StringValue
        {
            public StringValue() { Value = ""; }
            public StringValue(string v) { Value = v; }

            [XmlAttribute("value")]
            public string Value { get; set; }
        }

        public class IntValue
        {
            public IntValue() { Value = 0; }
            public IntValue(int v) { Value = v; }
            [XmlAttribute("value")]
            public int Value { get; set; }
        }


        public class DbServer
        {
            [XmlAttribute("host")]
            public string Host { get; set; }
            [XmlAttribute("port")]
            public int Port { get; set; }
        }

        public class DbUser
        {
            [XmlAttribute("login")]
            public string Login { get; set; }
            [XmlAttribute("password")]
            public string Password { get; set; }
        }

        public class DbTable
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class SqlCommand
        {
            [XmlAttribute("timeout")]
            public int Timeout { get; set; }
        }

        public class Database
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("type")]
            public string DbType { get; set; }
            [XmlElement]
            public DbServer Server { get; set; }
            [XmlElement]
            public DbUser User { get; set; }
            [XmlElement]
            public DbTable Table { get; set; }
            [XmlElement]
            public SqlCommand SqlCommand { get; set; }
        }


        public class LogFolder
        {
            [XmlAttribute("directory")]
            public string Directory { get; set; }

            [XmlAttribute("filter")]
            public string Filter { get; set; }
        }

        public class TableauRepo
        {
            [XmlAttribute]
            public string Host { get; set; }
            [XmlAttribute]
            public int Port { get; set; }
            [XmlAttribute]
            public string Username { get; set; }
            [XmlAttribute]
            public string Password { get; set; }
            [XmlAttribute]
            public string Db { get; set; }
        }

        public class Process
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class Host
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class Cluster
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlElement("Host")]
            public List<Host> Hosts { get; set; }
        }

        [XmlRoot(ElementName = "PalMonConfig", Namespace = "PalMon")]
        public class PaletteInsightConfiguration
        {

            [XmlElement]
            public StringValue OutputMode { get; set; }

            [XmlElement]
            public IntValue PollInterval { get; set; }
            [XmlElement]
            public IntValue LogPollInterval { get; set; }
            [XmlElement]
            public IntValue ThreadInfoPollInterval { get; set; }

            [XmlArray("Processes")]
            [XmlArrayItem("Process")]
            public List<Process> Processes { get; set; }

            [XmlArray("Clusters")]
            [XmlArrayItem("Cluster")]
            public List<Cluster> Clusters { get; set; }

            [XmlElement]
            public Database Database { get; set; }

            [XmlArray("Logs")]
            [XmlArrayItem("LogFolder")]
            public List<LogFolder> Logs { get; set; }

            [XmlElement]
            public TableauRepo TableauRepo;
        }

        #endregion
    }

}
