using System;
using System.Collections.Generic;
using PaletteInsightAgent;
using PaletteInsightAgent.Output;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using NLog;
using PaletteInsightAgent.Output.OutputDrivers;

namespace PaletteInsight
{
    namespace Configuration
    {
        /// <summary>
        /// A class for loading the configuration and converting it to
        /// the legacy PaletteInsightAgentOptions class.
        /// </summary>
        public class Loader
        {
            private const string LOGFOLDER_DEFAULTS_FILE = "Config/LogFolders.yml";
            private const string PROCESSES_DEFAULT_FILE = "Config/Processes.yml";
            private const string REPOSITORY_TABLES_FILE = "Config/Repository.yml";
            private static readonly Logger Log = LogManager.GetCurrentClassLogger();


            /// <summary>
            /// Do the conversion of config types
            /// </summary>
            /// <param name="conf"></param>
            /// <param name="outConfig">the PaletteInsightAgentOptions instance to update, since its a singleton, we cannot
            /// call its constructor, hence we cannot return it.</param>
            public static void LoadConfigTo(PaletteInsightConfiguration config, PaletteInsightAgent.PaletteInsightAgentOptions options)
            {
                options.PollInterval = config.PollInterval;
                options.LogPollInterval = config.LogPollInterval;
                options.RepoTablesPollInterval = config.RepoTablesPollInterval;
                options.ThreadInfoPollInterval = config.ThreadInfoPollInterval;
                options.DBWriteInterval = config.DBWriteInterval;

                options.ProcessedFilestTTL = config.ProcessedFilesTTL;
                options.StorageLimit = config.StorageLimit;

                options.AllProcesses = config.AllProcesses2;

                // store the result database details
                options.ResultDatabase = CreateDbConnectionInfo(config.Database);

                if (config.Webservice != null)
                {
                    // Do not add the username or password here, as they come from the license
                    options.WebserviceConfig = new WebserviceConfiguration
                    {
                        Endpoint = config.Webservice.Endpoint,
                        UseProxy = config.Webservice.UseProxy,
                        ProxyAddress = config.Webservice.ProxyAddress,
                        ProxyUsername = config.Webservice.ProxyUsername,
                        ProxyPassword = config.Webservice.ProxyPassword
                    };
                }
                else
                {
                    // make sure the webservice config is null, so we wont write
                    // to the webservice if its not configured
                    options.WebserviceConfig = null;
                }

                // Load thread monitoring configuration
                options.Processes = new Dictionary<string, ProcessData>();
                foreach (var process in LoadProcessData())
                {
                    options.Processes.Add(process.Name, process);
                }

                options.RepositoryTables = LoadRepositoryTables();

                // Add the log folders based on the Tableau Data path from the registry
                var tableauRoot = GetTableauRegistryString("Data");

                AddLogFoldersToOptions(config, options, tableauRoot);
                AddRepoToOptions(config, options, tableauRoot);


                // setup the polling options
                options.UseCounterSamples = config.UseCounterSamples.HasValue ? config.UseCounterSamples.Value : true;
                options.UseLogPolling = config.UseLogPolling.HasValue ? config.UseLogPolling.Value : true;
                options.UseThreadInfo = config.UseThreadInfo.HasValue ? config.UseThreadInfo.Value : true;
                // If the UseRepoPolling flag is not set,
                // handle the legacy case of having the repo poll interval set to 0 to
                // signal that the repo tables should not be polled
                if (!config.UseRepoPolling.HasValue)
                {
                    options.UseRepoPolling = !(options.RepoTablesPollInterval == 0);
                }
                else
                {
                    options.UseRepoPolling = config.UseRepoPolling.Value;
                }
            }

            public static void updateWebserviceConfigFromLicense(PaletteInsightAgent.PaletteInsightAgentOptions options, Licensing.License license)
            {
                // skip if we arent using the webservice
                if (options.WebserviceConfig == null) return;

                options.WebserviceConfig.Username = license.licenseId;
                options.WebserviceConfig.AuthToken = license.token;
            }

            /// <summary>
            /// Add the tableau repo database details to the options.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="options"></param>
            /// <param name="tableauRoot"></param>
            private static void AddRepoToOptions(PaletteInsightConfiguration config, PaletteInsightAgentOptions options, string tableauRoot)
            {
                Workgroup repo = null;
                string workgroupyml = @"tabsvc\config\workgroup.yml";

                var configFilePath = "";
                try
                {
                    configFilePath = Path.Combine(tableauRoot, workgroupyml);
                    using (var reader = File.OpenText(configFilePath))
                    {
                        repo = GetRepoFromWorkgroupYaml(tableauRoot);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Error while trying to load and parse YAML config from {0} -- {1}", configFilePath, e);
                }


                // if the repository from the config is a null, then configure the repo from
                // the config file.
                if (repo == null)
                {
                    // load the tableau repo properties
                    var repoProps = config.TableauRepo;
                    options.RepositoryDatabase = new DbConnectionInfo
                    {
                        Server = repoProps.Host,
                        Port = Convert.ToInt32(repoProps.Port),
                        Username = repoProps.User,
                        Password = repoProps.Password,
                        DatabaseName = repoProps.Database
                    };
                }
                else
                {
                    if (config.TableauRepo != null)
                    {
                        Log.Warn("Ignoring Tableau repo settings from config.yml.");
                    }
                    options.RepositoryDatabase = new DbConnectionInfo
                    {
                        Server = repo.Connection.Host,
                        Port = repo.Connection.Port,
                        Username = repo.Username,
                        Password = repo.Password,
                        DatabaseName = repo.Connection.DatabaseName
                    };
                }
            }

            #region log folders

            /// <summary>
            /// The log folders we are interested in, relative from the Tableau Data Root
            /// </summary>
            private static readonly LogFolder[] LOG_PATHS_IN_TABLEAU_DATA_FOLDER = new LogFolder[]
            {
                new LogFolder {Directory = @"tabsvc\vizqlserver\Logs", Filter = "*.txt" },
                new LogFolder {Directory =  @"tabsvc\logs\vizqlserver", Filter = "tabprotosrv*.txt" },
            };


            /// <summary>
            /// Adds the log folders from either the config (if no tableau server is installed),
            /// or from the registry
            /// </summary>
            /// <param name="config"></param>
            /// <param name="options"></param>
            /// <param name="tableauRoot"></param>
            private static void AddLogFoldersToOptions(PaletteInsightConfiguration config, PaletteInsightAgentOptions options, string tableauRoot)
            {
                // check and load the log folder paths from the config file, so
                // the folders listed in there will be definitely watched
                if (config.Logs != null)
                {
                    foreach (LogFolder logFolder in config.Logs)
                    {
                        // we just blindly add here, as this code path is only for debugging
                        options.LogFolders.Add(new PaletteInsightAgentOptions.LogFolderInfo
                        {
                            FolderToWatch = logFolder.Directory,
                            DirectoryFilter = logFolder.Filter
                        });
                    }
                }

                if (tableauRoot != null)
                {
                    // otherwise try to add the log folders from the registry setup
                    foreach (var logFolder in LoadDefaultLogFolders())
                    {
                        var fullPath = Path.Combine(tableauRoot, logFolder.Directory);
                        // we check here so we won't add non-existent folders
                        if (!Directory.Exists(fullPath))
                        {
                            Log.Error("Log folder not found: {0}", fullPath);
                            continue;
                        }
                        options.LogFolders.Add(new PaletteInsightAgentOptions.LogFolderInfo
                        {
                            FolderToWatch = fullPath,
                            DirectoryFilter = logFolder.Filter,
                        });
                    }    
                }
            }

            /// <summary>
            ///  Tries to load the default log folders from the log folders yaml file.
            ///  Since failing to load these disables parsing any logs, this
            ///  method throws its errors
            /// </summary>
            /// <returns></returns>
            private static List<LogFolder> LoadDefaultLogFolders()
            {
                // load the defaults from the application
                // since PaletteInsightAgent always sets the current directory to its location,
                // we should always be in the correct folder for this to work
                using (var reader = File.OpenText(LOGFOLDER_DEFAULTS_FILE))
                {
                    var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                    return deserializer.Deserialize<List<LogFolder>>(reader);
                }
            }

            #endregion

            /// <summary>
            /// Helper to create a database configuration 
            /// </summary>
            /// <param name="databaseConfig"></param>
            /// <returns></returns>
            private static IDbConnectionInfo CreateDbConnectionInfo(DatabaseConfig databaseConfig)
            {
                IDbConnectionInfo dbConnInfo = new DbConnectionInfo()
                {
                    Server = databaseConfig.Host,
                    Port = databaseConfig.Port,
                    Username = databaseConfig.User,
                    Password = databaseConfig.Password,
                    DatabaseName = databaseConfig.Database,
                    CommandTimeout = databaseConfig.CommandTimeout
                };


                if (!dbConnInfo.Valid())
                {
                    throw new ConfigurationErrorsException("Missing required database connection information!");
                }

                return dbConnInfo;
            }

            #region Tableau Registry info


            // A list of possible locations for the tableau data in the registry.
            // For now we are puttin this in descending version order so that the most recent
            // version installed will be returned.
            static readonly string[] POSSIBLE_TABLEAU_REGISTRY_PATHS = new string[]
            {
                    @"Software\Tableau\Tableau Server 9.3\Directories",
                    @"Software\Tableau\Tableau Server 9.2\Directories",
                    @"Software\Tableau\Tableau Server 9.1\Directories",
            };


            /// <summary>
            /// Tries to get a value as string from the registry from the installed Tableau Servers version
            /// </summary>
            /// <param name="subKey">The name of the value to get from the tableau version</param>
            /// <returns>null if no Tableau is found or the </returns>
            private static string GetTableauRegistryString(string subKey = "Data")
            {
                // Try all versions of tableau from highest to lowest
                using (var localKey = Environment.Is64BitOperatingSystem
                        ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    foreach (var regPath in POSSIBLE_TABLEAU_REGISTRY_PATHS)
                    {
                        try
                        {
                            using (RegistryKey key = localKey.OpenSubKey(regPath))
                            {
                                if (key == null) continue;
                                Object o = key.GetValue(subKey);
                                if (o == null) continue;
                                return o as String;
                            }
                        }
                        catch (Exception)
                        {
                            // no problem, only means this is not our version
                        }
                    }
                    return null;
                }
            }
            #endregion


            #region private Repository parts


            /// <summary>
            /// Deserialization struct for the repo config from the workgroup.yml
            /// </summary>
            public class Workgroup
            {
                [YamlMember(Alias = "pgsql.readonly.enabled")]
                public bool ReadonlyEnabled { get; set; }

                [YamlMember(Alias = "pgsql.readonly_username")]
                public string Username { get; set; }

                [YamlMember(Alias = "pgsql.readonly_password")]
                public string Password { get; set; }

                [YamlMember(Alias = "pgsql.connections.yml")]
                public string ConnectionsFile { get; set; }

                public TableauConnectionInfo Connection { get; set; }
            }

            public class TableauConnectionInfo
            {
                [YamlMember(Alias = "pgsql.host")]
                public string Host { get; set; }

                [YamlMember(Alias = "pgsql.port")]
                public int Port { get; set; }

                // It is not possible to change it @Tableau so we hardcode it for now
                public readonly string DatabaseName = "workgroup";
            }

            private static bool IsValidRepoData(Workgroup workgroup)
            {
                return workgroup.ReadonlyEnabled
                    && workgroup.Username != null
                    && workgroup.Password != null
                    && workgroup.Connection.Host != null;
            }

            private static Workgroup GetRepoFromWorkgroupYaml(string tableauRoot)
            {
                // Get basic info from workgroup yml. Everything else from connections.yml
                var deserializer = new Deserializer(namingConvention: new PascalCaseNamingConvention(), ignoreUnmatched: true);

                string workgroupyml = @"tabsvc\config\workgroup.yml";
                var configFilePath = Path.Combine(tableauRoot, workgroupyml);
                Workgroup workgroup = null;
                using (var workgroupFile = File.OpenText(configFilePath))
                {
                    workgroup = deserializer.Deserialize<Workgroup>(workgroupFile);
                    using (var connectionsFile = File.OpenText(workgroup.ConnectionsFile))
                    {
                        workgroup.Connection = deserializer.Deserialize<TableauConnectionInfo>(connectionsFile);
                    }
                    if (!IsValidRepoData(workgroup))
                    {
                        return null;
                    }
                }

                return workgroup;
            }

            #endregion

            #region process defaults


            /// <summary>
            ///  Tries to load the default process names from the process names yaml file.
            ///  Since failing to load these disables parsing any processs, this
            ///  method throws its errors
            /// </summary>
            /// <returns></returns>
            private static List<ProcessData> LoadProcessData()
            {
                // since PaletteInsightAgent always sets the current directory to its location,
                // we should always be in the correct folder for this to work
                using (var reader = File.OpenText(PROCESSES_DEFAULT_FILE))
                {
                    var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                    return deserializer.Deserialize<List<ProcessData>>(reader);
                }
            }

            private static List<RepoTable> LoadRepositoryTables()
            {
                using (var reader = File.OpenText(REPOSITORY_TABLES_FILE))
                {
                    var deserializer = new Deserializer(namingConvention: new NullNamingConvention());
                    return deserializer.Deserialize<List<RepoTable>>(reader);
                }
            }


            #endregion
        }
    }
}
