using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PaletteConfigurator
{
    public class InsightAgentConfiguration
    {
        public string AgentFolder { get; set; }
        public DbDetails ResultsDatabase { get; set; }
        public DbDetails TableauRepo { get; set; }

        public string LogWatchFolder { get; set; }
        public string LogWatchMask { get; set; }


        public int PollInterval { get; set; }
        public int LogPollInterval { get; set; }
        public int ThreadInfoPollInterval { get; set; }

        public List<ClusterData> Clusters { get; set; }
        public List<string> Processes { get; set; }

    }

    public class ClusterData
    {
        public string ClusterName { get; set; }
        public IList<string> Nodes { get; set; }
    }



    #region PalMon.Config description

    namespace PalMonConf
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
        }


        public class LogPollerConfig
        {
            [XmlAttribute]
            public string Directory { get; set; }

            [XmlAttribute]
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

        [XmlRoot(ElementName = "PalMonConfig", Namespace ="PalMon")]
        public class PalMonConfiguration
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

            [XmlElement]
            public LogPollerConfig LogPoller { get; set; }

            [XmlElement]
            public TableauRepo TableauRepo;
        }



        public class ConfigConverter
        {

            /// <summary>
            /// Convert a regular configuration to the agents weird XML config
            /// </summary>
            /// <param name="cfg"></param>
            /// <returns></returns>
            public static PalMonConfiguration ConfigToPalMonConfig(InsightAgentConfiguration cfg)
            {
                return new PalMonConfiguration
                {
                    OutputMode = new StringValue("db"),
                    PollInterval = new IntValue(cfg.PollInterval),
                    LogPollInterval = new IntValue(cfg.LogPollInterval),
                    ThreadInfoPollInterval = new IntValue(cfg.ThreadInfoPollInterval),
                    Processes = cfg.Processes.Select(x => new Process { Name = x }).ToList(),
                    Clusters = cfg.Clusters.Select(x => new Cluster {
                        Name = x.ClusterName,
                        Hosts = x.Nodes.Select( node => new Host { Name = node }).ToList()
                    }).ToList(),
                    
                    Database = new Database
                    {
                        Name = cfg.ResultsDatabase.Database,
                        DbType = cfg.ResultsDatabase.DbType,
                        Server = new DbServer
                        {
                            Host = cfg.ResultsDatabase.Host,
                            Port = cfg.ResultsDatabase.Port
                        },
                        User = new DbUser
                        {
                            Login = cfg.ResultsDatabase.Username,
                            Password = cfg.ResultsDatabase.Password
                        },
                        Table = new DbTable
                        {
                            Name = "countersamples"
                        }
                    },
                    TableauRepo = new TableauRepo
                    {
                        Host = cfg.TableauRepo.Host,
                        Port = cfg.TableauRepo.Port,
                        Username = cfg.TableauRepo.Username,
                        Password = cfg.TableauRepo.Password,
                        Db = cfg.TableauRepo.Database
                    },
                    LogPoller = new LogPollerConfig
                    {
                        Directory = cfg.LogWatchFolder,
                        Filter = cfg.LogWatchMask
                    }
                };
            }


            public static InsightAgentConfiguration FromPalMonConfig(string agentFolder, PalMonConfiguration conf)
            {
                return new InsightAgentConfiguration
                {
                    ThreadInfoPollInterval = conf.ThreadInfoPollInterval.Value,
                    LogPollInterval = conf.LogPollInterval.Value,
                    PollInterval = conf.PollInterval.Value,

                    AgentFolder = agentFolder,
                    LogWatchFolder = conf.LogPoller.Directory,
                    LogWatchMask = conf.LogPoller.Filter,

                    TableauRepo = new DbDetails
                    {
                        Host = conf.TableauRepo.Host,
                        Port = conf.TableauRepo.Port,
                        Username = conf.TableauRepo.Username,
                        Password = conf.TableauRepo.Password,
                        Database = conf.TableauRepo.Db
                    },

                    ResultsDatabase = new DbDetails
                    {
                        Host = conf.Database.Server.Host,
                        Port = conf.Database.Server.Port,
                        Username = conf.Database.User.Login,
                        Password = conf.Database.User.Password,
                        Database = conf.Database.Name
                    },
                    Clusters = conf.Clusters.Select( cluster => new ClusterData
                        {
                            ClusterName = cluster.Name,
                            Nodes = cluster.Hosts.Select(host => host.Name).ToList()
                        }).ToList(),

                    Processes = conf.Processes.Select(proc => proc.Name).ToList()


                   


                };
            }
        }
    }

    #endregion
}
