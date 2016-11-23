﻿using System;
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
using System.Text.RegularExpressions;

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
                options.UploadInterval = config.UploadInterval;
                options.RepoTablesPollInterval = config.RepoTablesPollInterval;
                options.StreamingTablesPollInterval = config.StreamingTablesPollInterval;
                options.ThreadInfoPollInterval = config.ThreadInfoPollInterval;

                options.ProcessedFilestTTL = config.ProcessedFilesTTL;
                options.StorageLimit = config.StorageLimit;

                options.AllProcesses = config.AllProcesses2;

                options.LicenseKey = config.LicenseKey;

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
                var tableauRoot = GetTableauDataFolder();

                AddLogFoldersToOptions(config, options, tableauRoot);
                AddRepoToOptions(config, options, tableauRoot);

                // setup the polling options
                options.UseCounterSamples = config.UseCounterSamples;
                options.UseLogPolling = config.UseLogPolling;
                options.UseThreadInfo = config.UseThreadInfo;

                options.IsPrimaryNode = config.IsPrimaryNode;

                // Polling of Tableau repo and streaming tables needs to be executed only on primary nodes.
                // [...] for the legacy case UseRepoPolling is true by default and RepoTablesPollInterval is 0 to
                // disable repo polling so this would mean different behaviour with the same config file.
                options.UseRepoPolling = config.IsPrimaryNode && config.UseRepoPolling && config.RepoTablesPollInterval > 0;
                // and streaming tables is very similar and related to repo polling
                options.UseStreamingTables = config.IsPrimaryNode && config.UseRepoPolling && config.StreamingTablesPollInterval > 0;

                // set the maximum log lines
                options.LogLinesPerBatch = config.LogLinesPerBatch;
            }

            public static PaletteInsightConfiguration LoadConfigFile(string filename)
            {
                try
                {
                    // deserialize the config
                    using (var reader = File.OpenText(filename))
                    {
                        var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                        var config = deserializer.Deserialize<PaletteInsightConfiguration>(reader);
                        return config;
                    }
                }
                catch (Exception e)
                {
                    Log.Fatal("Error during cofiguration loading: {0} -- {1}", filename, e);
                    return null;
                }
            }

            public static void UpdateWebserviceConfigFromLicense(PaletteInsightAgent.PaletteInsightAgentOptions options)
            {
                // skip if we arent using the webservice
                if (options.WebserviceConfig == null) return;

                options.WebserviceConfig.AuthToken = options.LicenseKey;
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

                var configFilePath = "";
                try
                {
                    configFilePath = Path.Combine(tableauRoot, "tabsvc", "config", "workgroup.yml");
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
            /// Adds a watched folder to the list of watched folders.
            /// </summary>
            /// <returns>If the folder was added</returns>
            private static bool AddFolderToWatched(ICollection<PaletteInsightAgentOptions.LogFolderInfo> logFolders, PaletteInsightAgentOptions.LogFolderInfo folder)
            {
                var folderValueString = folder.ToValueString();
                foreach (var logFolder in logFolders)
                {
                    // Skip if we already have this folder
                    if (String.Equals(logFolder.ToValueString(), folderValueString))
                    {
                        Log.Error("Skipping addition of duplicate watched path: {0}", folderValueString);
                        return false;
                    }
                }

                // If no matches found, add to the list of dirs
                logFolders.Add(folder);
                Log.Info("Watching folder: {0} with filter: {1} with format: {2}", folder.FolderToWatch, folder.DirectoryFilter, folder.LogFormat);
                return true;
            }

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
                        AddFolderToWatched(options.LogFolders, 
                            PaletteInsightAgentOptions.LogFolderInfo.Create( logFolder.Directory, logFolder.Filter, logFolder.Format ));
                    }
                }

                if (tableauRoot != null)
                {
                    // otherwise try to add the log folders from the registry setup
                    foreach (var logFolder in LoadDefaultLogFolders())
                    {
                        var fullPath = Path.GetFullPath(Path.Combine(tableauRoot, logFolder.Directory));
                        // we check here so we won't add non-existent folders
                        if (!Directory.Exists(fullPath))
                        {
                            Log.Error("Log folder not found: {0}", fullPath);
                            continue;
                        }
                        
                        AddFolderToWatched(options.LogFolders,
                            PaletteInsightAgentOptions.LogFolderInfo.Create(fullPath, logFolder.Filter, logFolder.Format));
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
            private static DbConnectionInfo CreateDbConnectionInfo(DatabaseConfig databaseConfig)
            {
                DbConnectionInfo dbConnInfo = new DbConnectionInfo()
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

            /// <summary>
            /// Tries to read Tableau's data folder from the registry
            /// </summary>
            /// <returns>null if no Tableau data folder is found in the registry </returns>
            private static string GetTableauDataFolder()
            {
                // Try all versions of tableau from highest to lowest
                using (var localKey = Environment.Is64BitOperatingSystem
                        ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    RegistryKey tableauKey = localKey.OpenSubKey(@"Software\Tableau");
                    if (tableauKey == null)
                    {
                        return null;
                    }

                    Version latestTableauVersion = new Version("0.0");
                    string tableauDataFolder = null;

                    foreach (string key in tableauKey.GetSubKeyNames())
                    {
                        var pattern = new Regex(@"^Tableau Server ([0-9]+\.[0-9]+)");
                        var groups = pattern.Match(key).Groups;
                        // groups[0] is the entire match, thus we expect 2
                        if (groups.Count < 2)
                        {
                            continue;
                        }

                        Version version = new Version(groups[1].Value);
                        if (version > latestTableauVersion)
                        {
                            try
                            {
                                string directoriesRegPath = Path.Combine(key, "Directories");
                                using (RegistryKey dataFolderKey = tableauKey.OpenSubKey(directoriesRegPath))
                                {
                                    if (dataFolderKey == null)
                                    {
                                        continue;
                                    }
                                    Object dataValue = dataFolderKey.GetValue("Data");
                                    if (dataValue == null)
                                    {
                                        continue;
                                    }
                                    latestTableauVersion = version;
                                    tableauDataFolder = dataValue as String;
                                }
                            }
                            catch (Exception)
                            {
                                // no problem, only means this is not our version
                                continue;
                            }
                        }
                    }

                    if (tableauDataFolder == null)
                    {
                        Log.Error("Failed to determine version of Tableau Server!");
                        return null;
                    }

                    Log.Info("Found Tableau Data folder: {0}", tableauDataFolder);
                    return tableauDataFolder;
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

                var configFilePath = Path.Combine(tableauRoot, "tabsvc", "config", "workgroup.yml");
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
