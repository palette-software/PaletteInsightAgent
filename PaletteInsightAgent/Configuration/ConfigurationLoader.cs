using System;
using System.Collections.Generic;
using PaletteInsightAgent.Output;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using NLog;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Output.OutputDrivers;
using System.Text.RegularExpressions;
using System.Management;

namespace PaletteInsightAgent
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
            private const string TABLEAU_SERVER_APPLICATION_SERVICE_NAME = "tabsvc";
            private static readonly Logger Log = LogManager.GetCurrentClassLogger();


            /// <summary>
            /// Do the conversion of config types
            /// </summary>
            /// <param name="conf"></param>
            /// <param name="outConfig">the PaletteInsightAgentOptions instance to update, since its a singleton, we cannot
            /// call its constructor, hence we cannot return it.</param>
            public static void LoadConfigTo(PaletteInsightConfiguration config, string tableauRoot, PaletteInsightAgentOptions options)
            {
                options.PollInterval = config.PollInterval;
                options.LogPollInterval = config.LogPollInterval;
                options.UploadInterval = config.UploadInterval;
                options.RepoTablesPollInterval = config.RepoTablesPollInterval;
                options.StreamingTablesPollInterval = config.StreamingTablesPollInterval;
                options.ThreadInfoPollInterval = config.ThreadInfoPollInterval;

                options.ProcessedFilestTTL = config.ProcessedFilesTTL;
                options.StorageLimit = config.StorageLimit;

                options.AllProcesses = config.AllProcesses;

                options.AuthToken = config.InsightAuthToken;

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
                AddLogFoldersToOptions(config, options, tableauRoot);

                // setup the polling options
                options.UseCounterSamples = config.UseCounterSamples;
                options.UseLogPolling = config.UseLogPolling;
                options.UseThreadInfo = config.UseThreadInfo;

                // Polling of Tableau repo and streaming tables needs to be executed only on primary nodes.
                // [...] for the legacy case UseRepoPolling is true by default and RepoTablesPollInterval is 0 to
                // disable repo polling so this would mean different behaviour with the same config file.
                options.UseRepoPolling = config.UseRepoPolling && config.RepoTablesPollInterval > 0;
                // and streaming tables is very similar and related to repo polling
                options.UseStreamingTables = config.UseRepoPolling && config.StreamingTablesPollInterval > 0;
                LoadRepositoryFromConfig(config, options);

                // set the maximum log lines
                options.LogLinesPerBatch = config.LogLinesPerBatch;
            }

            private static void LoadRepositoryFromConfig(PaletteInsightConfiguration config, PaletteInsightAgentOptions options)
            {
                if (!options.UseRepoPolling && !options.UseStreamingTables)
                {
                    return;
                }

                // load the tableau repo properties
                var repoProps = config.TableauRepo;
                if (repoProps == null)
                {
                    // Repository credentials are not filled in Config.yml
                    return;
                }

                try
                {
                    options.RepositoryDatabase = new DbConnectionInfo
                    {
                        Server = repoProps.Host,
                        Port = Convert.ToInt32(repoProps.Port),
                        Username = repoProps.User,
                        Password = repoProps.Password,
                        DatabaseName = repoProps.Database
                    };

                    Log.Info("Found Tableau repo credentials in Config.yml.");
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to parse Tableau repository configs! Error:");
                }
            }

            public static PaletteInsightConfiguration LoadConfigFile(string filename)
            {
                try
                {
                    // deserialize the config
                    using (var reader = File.OpenText(filename))
                    {
                        Deserializer deserializer = YamlDeserializer.Create(new UnderscoredNamingConvention());
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

            public static void UpdateWebserviceConfigFromLicense(PaletteInsightAgentOptions options)
            {
                // skip if we arent using the webservice
                if (options.WebserviceConfig == null) return;

                options.WebserviceConfig.AuthToken = options.AuthToken;
            }

            /// <summary>
            /// Add the tableau repo database details to the options.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="options"></param>
            /// <param name="tableauRoot"></param>
            public static bool AddRepoFromWorkgroupYaml(PaletteInsightConfiguration config, string tableauRoot, PaletteInsightAgentOptions options)
            {
                options.PreferPassiveRepo = config.PreferPassiveRepository;

                Workgroup repo = GetRepoFromWorkgroupYaml(GetWorkgroupYmlPath(), options.PreferPassiveRepo);
                if (repo == null)
                {
                    return false;
                }

                try
                {
                    if (IsEncrypted(repo.Password))
                    {
                        Log.Info("Encrypted readonly password found in workgroup.yml. Getting password with tabadmin command.");
                        repo.Password = Tableau.tabadminRun("get pgsql.readonly_password");
                    }
                    options.RepositoryDatabase = new DbConnectionInfo
                    {
                        Server = repo.Connection.Host,
                        Port = repo.Connection.Port,
                        Username = repo.Username,
                        Password = repo.Password,
                        DatabaseName = repo.Connection.DatabaseName
                    };

                    if (config.TableauRepo != null)
                    {
                        Log.Warn("Ignoring Tableau repo settings from config.yml.");
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to acquire Tableau repo credentials! Exception: ");
                }

                return false;
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
                    Deserializer deserializer = YamlDeserializer.Create(new UnderscoredNamingConvention());
                    return deserializer.Deserialize<List<LogFolder>>(reader);
                }
            }


            internal static bool IsEncrypted(string text)
            {
                var pattern = new Regex(@"^ENC\(.*\)$");
                var matched = pattern.Match(text);
                return matched.Success;
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
            /// Retrieve Tableau's data folder path from the registry or figure it out
            /// based on its installation folder or fallback and try the usual path.
            /// </summary>
            /// <returns></returns>
            public static string FindTableauDataFolder()
            {
                // Primary nodes store the data folder location in the registry, except if
                // Tableau Server is installed on C: drive
                string dataFolderPath = SearchRegistryForTableauDataFolder();
                if (dataFolderPath != null)
                {
                    Log.Info("Found Tableau Data folder in registry: {0}", dataFolderPath);
                    return dataFolderPath;   
                }

                // Look for it in the Tableau installation folder
                dataFolderPath = SearchDataFolderInInstallationFolder();
                if (dataFolderPath != null)
                {
                    Log.Info("Found Tableau Data folder in installation folder: {0}", dataFolderPath);
                    return dataFolderPath;
                }

                // Try the usual path as a last resort. The data folder is located here if you install
                // Tableau Server on drive C: according to Tableau documentation
                dataFolderPath = @"C:\ProgramData\Tableau\Tableau Server\data";
                if (Directory.Exists(dataFolderPath))
                {
                    Log.Info("Found Tableau Data folder at default location: {0}", dataFolderPath);
                    return dataFolderPath;
                }

                // No luck at all
                Log.Error("Could not find Tableau data folder!");
                return null;
            }

            /// <summary>
            /// Tries to read Tableau's data folder from the registry
            /// NOTE: This function works as long as Tableau versions are in X.X format
            /// </summary>
            /// <returns>null if no Tableau data folder is found in the registry </returns>
            private static string SearchRegistryForTableauDataFolder()
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

                                    string possibleDataFolder = dataValue as String;
                                    if (!Directory.Exists(possibleDataFolder))
                                    {
                                        continue;
                                    }
                                    latestTableauVersion = version;
                                    tableauDataFolder = possibleDataFolder;
                                }
                            }
                            catch (Exception)
                            {
                                // no problem, only means this is not our version
                                continue;
                            }
                        }
                    }
                    tableauKey.Close();

                    if (tableauDataFolder == null)
                    {
                        return null;
                    }

                    return tableauDataFolder;
                }
            }

            /// <summary>
            /// Try to find the Tableau data folder in the Tableau Installation folder,
            /// which is calculated based on the path of the Tableau Server Application
            /// Manager (tabsvc) service.
            /// 
            /// For worker nodes this is the way we can discover Tableau data folder, if
            /// it is not located in the default directory.
            /// </summary>
            /// <returns></returns>
            private static string SearchDataFolderInInstallationFolder()
            {
                string tableauInstallFolder = RetrieveTableauInstallationFolder();
                if (tableauInstallFolder == null)
                {
                    Log.Warn("Could not find Tableau installation folder!");
                    return null;
                }

                string dataFolderPath = Path.Combine(tableauInstallFolder, "data");
                if (!Directory.Exists(dataFolderPath))
                {
                    return null;
                }

                return dataFolderPath;
            }

            private static string RetrieveTableauInstallationFolder()
            {
                string tabsvcPath = GetPathOfService(TABLEAU_SERVER_APPLICATION_SERVICE_NAME);
                return ExtractTableauInstallationFolder(tabsvcPath);
            }

            public static string RetrieveTableauBinFolder()
            {
                string tabsvcPath = GetPathOfService(TABLEAU_SERVER_APPLICATION_SERVICE_NAME);
                return ExtractTableauBinFolder(tabsvcPath);
            }

            public static string GetWorkgroupYmlPath()
            {
                string tabsvcPath = GetPathOfService(TABLEAU_SERVER_APPLICATION_SERVICE_NAME);
                var matchGroups = ParseTabsvcPath(tabsvcPath);
                if (matchGroups == null)
                {
                    Log.Warn("Failed to get workgroup.yml path from: {0}", tabsvcPath);
                    return null;
                }

                string tableauInstallFolder = matchGroups[1].Value;
                string workgroupYmlPath = Path.Combine(tableauInstallFolder, "data", "tabsvc", "config");
                if (matchGroups.Count == 3)
                {
                    string tabsvcVersionFolder = matchGroups[2].Value;
                    workgroupYmlPath = Path.Combine(workgroupYmlPath, tabsvcVersionFolder);
                }
                workgroupYmlPath = Path.Combine(workgroupYmlPath, "workgroup.yml");

                if (!File.Exists(workgroupYmlPath)) {
                    Log.Error("Failed to find workgroup.yml file at at '{0}'!", workgroupYmlPath);
                    return null;
                }

                return workgroupYmlPath;
            }

            // This function is created only for unit testing, since it is pretty difficult
            // to mock static functions in C#
            internal static string ExtractTableauInstallationFolder(string tabsvcPath)
            {
                var matchGroups = ParseTabsvcPath(tabsvcPath);
                if (matchGroups == null)
                {
                    Log.Warn("Failed to extract Tableau installation folder from: {0}", tabsvcPath);
                    return null;
                }

                return matchGroups[1].Value;
            }

            internal static GroupCollection ParseTabsvcPath(string tabsvcPath)
            {
                if (tabsvcPath == null)
                {
                    Log.Warn("Failed to extract Tableau Installation folder as the path to 'tabsvc' service is null!");
                    return null;
                }

                // Extract the installation folder out of the tabsvc path. We are going to
                // chop <one_folder>\bin\tabsvc.exe from the end of the tabsvc path.
                var pattern = new Regex(@"""?(.*?)[\\\/]+[^\\\/]+[\\\/]+bin[\\\/]+tabsvc.exe.*");
                var groups = pattern.Match(tabsvcPath).Groups;
                // groups[0] is the entire match, thus we expect at least 2
                if (groups.Count < 2)
                {
                    // Onwards Tableau Server 2018.2 the tabsvc.exe location is slightly different. In this case
                    // we are going to chop data\tabsvc\services\<tabsvc_version_folder>\tabsvc\tabsvc.exe from the
                    // end of the tabsvc path.
                    pattern = new Regex(@"""?(.*?)[\\\/]+data[\\\/]+tabsvc[\\\/]+services[\\\/]+([^\\\/]+)[\\\/]+tabsvc[\\\/]+tabsvc.exe.*");
                    groups = pattern.Match(tabsvcPath).Groups;
                    if (groups.Count < 3)
                    {
                        Log.Warn("Failed to extract Tableau Installation folder from 'tabsvc' path: '{0}'", tabsvcPath);
                        return null;
                    }
                }

                return groups;
            }

           // This function is created only for unit testing, since it is pretty difficult
           // to mock static functions in C#
           internal static string ExtractTableauBinFolder(string tabsvcPath)
            {
                if (tabsvcPath == null)
                {
                    Log.Warn("Failed to extract Tableau bin folder as the path to 'tabsvc' service is null!");
                    return null;
                }

                // Extract the installation folder out of the tabsvc path. We are going to
                // chop tabsvc.exe  from the end of the tabsvc path.
                var pattern = new Regex(@"""?(.*[\\\/]+bin[\\\/]+?)tabsvc.exe.*");
                var groups = pattern.Match(tabsvcPath).Groups;
                // groups[0] is the entire match, thus we expect at least 2
                if (groups.Count < 2)
                {
                    Log.Warn("Failed to extract Tableau bin folder from 'tabsvc' path: '{0}'", tabsvcPath);
                    return null;
                }

                return groups[1].Value;
            }

            // This function is acquired from StackOverflow:
            // http://stackoverflow.com/questions/2728578/how-to-get-phyiscal-path-of-windows-service-using-net
            // (with a bit of more careful object disposal)
            public static string GetPathOfService(string serviceName)
            {
                WqlObjectQuery wqlObjectQuery = new WqlObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name like '{0}%'", serviceName));
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery))
                using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                {
                    foreach (ManagementObject managementObject in managementObjectCollection)
                    {
                        return managementObject.GetPropertyValue("PathName").ToString();
                    }
                }

                return null;
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

                [YamlMember(Alias = "pgsql0.host")]
                public string PgHost0 { get; set; }

                [YamlMember(Alias = "pgsql0.port")]
                public int PgPort0 { get; set; }

                [YamlMember(Alias = "pgsql1.host")]
                public string PgHost1 { get; set; }

                [YamlMember(Alias = "pgsql1.port")]
                public int PgPort1 { get; set; }

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
                if (!workgroup.ReadonlyEnabled)
                {
                    Log.Warn("Readonly user is not enabled! Repo credentials must be entered into Config.yml.");
                    return false;
                }

                if (workgroup.Username == null)
                {
                    Log.Error("Tableau repo username is null! Repo credentials must be entered into Config.yml.");
                    return false;
                }

                if (workgroup.Password == null)
                {
                    Log.Error("Tableau repo password is null! Repo credentials must be entered into Config.yml.");
                    return false;
                }

                if (workgroup.Connection.Host == null)
                {
                    Log.Error("Tableau repo hostname is null! Repo credentials must be entered into Config.yml.");
                    return false;
                }

                return true;
            }

            public static Workgroup GetRepoFromWorkgroupYaml(string workgroupYmlPath, bool preferPassiveRepo)
            {
                if (workgroupYmlPath == null)
                {
                    Log.Error("Path for workgroup.yml must not be null while reading configs!");
                    return null;
                }

                try
                {
                    // Get basic info from workgroup yml. Everything else from connections.yml
                    Deserializer deserializer = YamlDeserializer.Create(new PascalCaseNamingConvention());

                    Workgroup workgroup = null;
                    using (var workgroupFile = File.OpenText(workgroupYmlPath))
                    {
                        workgroup = deserializer.Deserialize<Workgroup>(workgroupFile);
                        using (var connectionsFile = File.OpenText(workgroup.ConnectionsFile))
                        {
                            workgroup.Connection = deserializer.Deserialize<TableauConnectionInfo>(connectionsFile);
                            // workgroup.Connection.Host always contains the active repo
                            if (preferPassiveRepo && workgroup.PgHost0 != null && workgroup.PgHost1 != null)
                            {
                                // Use passive repo if possible/exists
                                if (workgroup.Connection.Host != workgroup.PgHost0)
                                {
                                    workgroup.Connection.Host = workgroup.PgHost0;
                                    workgroup.Connection.Port = workgroup.PgPort0;
                                }
                                else
                                {
                                    workgroup.Connection.Host = workgroup.PgHost1;
                                    workgroup.Connection.Port = workgroup.PgPort1;
                                }
                                Log.Info("Using passive repository Host: '{0}' Port: '{1}'", workgroup.Connection.Host, workgroup.Connection.Port);
                            }
                        }
                        if (!IsValidRepoData(workgroup))
                        {
                            return null;
                        }
                    }

                    return workgroup;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while trying to load and parse YAML config from '{0}' Exception: ", workgroupYmlPath);
                    return null;
                }
            }

            #endregion

            #region process defaults


            /// <summary>
            ///  Tries to load the default process names from the process names yaml file.
            ///  Since failing to load these disables parsing any processs, this
            ///  method throws its errors
            /// </summary>
            /// <returns></returns>
            internal static List<ProcessData> LoadProcessData()
            {
                // since PaletteInsightAgent always sets the current directory to its location,
                // we should always be in the correct folder for this to work
                using (var reader = File.OpenText(PROCESSES_DEFAULT_FILE))
                {
                    Deserializer deserializer = YamlDeserializer.Create(new UnderscoredNamingConvention());
                    return deserializer.Deserialize<List<ProcessData>>(reader);
                }
            }

            private static List<RepoTable> LoadRepositoryTables()
            {
                using (var reader = File.OpenText(REPOSITORY_TABLES_FILE))
                {
                    Deserializer deserializer = YamlDeserializer.Create(new NullNamingConvention());
                    return deserializer.Deserialize<List<RepoTable>>(reader);
                }
            }


            #endregion
        }
    }
}
