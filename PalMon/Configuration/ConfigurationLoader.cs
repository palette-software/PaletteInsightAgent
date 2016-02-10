using System;
using System.Collections.Generic;
using PalMon;
using PalMon.Output;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using NLog;

namespace PaletteInsight
{
    namespace Configuration
    {
        /// <summary>
        /// A class for loading the configuration and converting it to
        /// the legacy PalMonOptions class.
        /// </summary>
        public class Loader
        {
            private const string LOGFOLDER_DEFAULTS_FILE = "Config/LogFolders.yml";
            private const string PROCESSES_DEFAULT_FILE = "Config/Processes.yml";
            private static readonly Logger Log = LogManager.GetCurrentClassLogger();


            /// <summary>
            /// Do the conversion of config types
            /// </summary>
            /// <param name="conf"></param>
            /// <param name="outConfig">the PalMonOptions instance to update, since its a singleton, we cannot
            /// call its constructor, hence we cannot return it.</param>
            public static void LoadConfigTo(PaletteInsightConfiguration config, PalMon.PalMonOptions options)
            {
                options.PollInterval = config.PollInterval;
                // Load LogPollInterval.
                options.LogPollInterval = config.LogPollInterval;

                // Load ThreadInfoPollInterval.
                options.ThreadInfoPollInterval = config.ThreadInfoPollInterval;
                // store the result database details
                options.ResultDatabase = CreateDbConnectionInfo(config.Database);


                // Load thread monitoring configuration
                options.Processes = new System.Collections.Generic.List<string>();
                foreach (var processName in LoadDefaultProcessNames())
                {
                    options.Processes.Add(processName);
                }

                foreach (var process in config.Processes)
                {
                    options.Processes.Add(process);
                }

                // Add the log folders based on the Tableau Data path from the registry
                var tableauRoot = GetTableauRegistryString("Data");

                AddLogFoldersToOptions(config, options, tableauRoot);
                AddRepoToOptions(config, options, tableauRoot);

            }

            /// <summary>
            /// Add the tableau repo database details to the options.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="options"></param>
            /// <param name="tableauRoot"></param>
            private static void AddRepoToOptions(PaletteInsightConfiguration config, PalMonOptions options, string tableauRoot)
            {
                Repository repo = null;
                string workgroupyml = @"tabsvc\config\workgroup.yml";

                try
                {
                    var configFilePath = Path.Combine(tableauRoot, workgroupyml);
                    using (var reader = File.OpenText(configFilePath))
                    {
                        repo = GetRepoFromWorkgroupYaml(reader);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Error while trying to load and parse YAML config from {0}/{1} -- {2}", tableauRoot, workgroupyml, e);
                }


                // if the repository from the config is a null, then configure the repo from
                // the config file.
                if (repo == null)
                {
                    // load the tableau repo properties
                    var repoProps = config.TableauRepo;
                    options.RepoHost = repoProps.Host;
                    options.RepoPort = Convert.ToInt32(repoProps.Port);
                    options.RepoUser = repoProps.User;
                    options.RepoPass = repoProps.Password;
                    options.RepoDb = repoProps.Name;
                }
                else
                {
                    options.RepoHost = repo.Host;
                    options.RepoPort = repo.Port0;
                    options.RepoUser = repo.Username;
                    options.RepoPass = repo.Password;
                    options.RepoDb = repo.DatabaseName;
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
            private static void AddLogFoldersToOptions(PaletteInsightConfiguration config, PalMonOptions options, string tableauRoot)
            {


                // if tableau server is not installed
                if (tableauRoot == null)
                {
                    // check and load the log folder paths from the config file, so
                    // we can still debug it without installing Tableau Server
                    foreach (LogFolder logFolder in config.Logs)
                    {
                        // we just blindly add here, as this code path is only for debugging
                        options.LogFolders.Add(new PalMonOptions.LogFolderInfo
                        {
                            FolderToWatch = logFolder.Directory,
                            DirectoryFilter = logFolder.Filter
                        });
                    }

                    return;
                }

                // otherwiser try to add the log folders from the registry setup
                foreach (var logFolder in LoadDefaultLogFolders())
                {
                    var fullPath = Path.Combine(tableauRoot, logFolder.Directory);
                    // we check here so we wont add non-existent folders
                    if (!Directory.Exists(fullPath)) continue;
                    options.LogFolders.Add(new PalMonOptions.LogFolderInfo
                    {
                        FolderToWatch = fullPath,
                        DirectoryFilter = logFolder.Filter,
                    });
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
                // since PalMonAgent always sets the current directory to its location,
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
            private static IDbConnectionInfo CreateDbConnectionInfo(Database databaseConfig)
            {
                IDbConnectionInfo dbConnInfo = new DbConnectionInfo()
                {
                    Server = databaseConfig.Host,
                    Port = databaseConfig.Port,
                    Username = databaseConfig.User,
                    Password = databaseConfig.Password,
                    DatabaseName = databaseConfig.Name,
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
            public class Repository
            {
                [YamlMember(Alias = "datacollector.postgres.host")]
                public string Host { get; set; }

                [YamlMember(Alias = "pgsql.readonly_username")]
                public string Username { get; set; }

                [YamlMember(Alias = "pgsql.readonly_password")]
                public string Password { get; set; }

                [YamlMember(Alias = "datacollector.postgres.tablename")]
                public string DatabaseName { get; set; }

                [YamlMember(Alias = "pgsql0.port")]
                public int Port0 { get; set; }

                // The testing tableau server had two ports in the workgroup.yml, hence this one
                [YamlMember(Alias = "pgsql1.port")]
                public int Port1 { get; set; }


            }

            private static Repository GetRepoFromWorkgroupYaml(TextReader input)
            {
                var deserializer = new Deserializer(namingConvention: new PascalCaseNamingConvention(), ignoreUnmatched: true);
                var result = deserializer.Deserialize<Repository>(input);
                return result;
            }

            #endregion

            #region process defaults


            /// <summary>
            ///  Tries to load the default process names from the process names yaml file.
            ///  Since failing to load these disables parsing any processs, this
            ///  method throws its errors
            /// </summary>
            /// <returns></returns>
            private static List<string> LoadDefaultProcessNames()
            {
                // load the defaults from the application
                // since PalMonAgent always sets the current directory to its location,
                // we should always be in the correct folder for this to work
                using (var reader = File.OpenText(PROCESSES_DEFAULT_FILE))
                {
                    var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                    return deserializer.Deserialize<List<string>>(reader);
                }
            }


            #endregion
        }
    }
}
