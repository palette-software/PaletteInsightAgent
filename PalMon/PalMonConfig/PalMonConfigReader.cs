using NLog;
using System;
using System.Configuration;
using PalMon.Helpers;
using PalMon.Output;

namespace PalMon.Config
{
    /// <summary>
    /// Parses the main application config to initialize an instance of PalMonOptions.
    /// </summary>
    public static class PalMonConfigReader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #region Public Methods

        /// <summary>
        /// Load PalMon.config and parse it into the PalMonOptions singleton, initializing writers if necessary.
        /// </summary>
        public static void LoadOptions()
        {
            var options = PalMonOptions.Instance;

            // Read PalMon.config
            Log.Info("Loading PalMon user configuration..");
            Configuration configFile;
            try
            {
                configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Fatal(ex, "Could not open configuration file: {0}", ex);
                throw;
            }

            // Parse config.
            try
            {
                var config = (PalMonConfig)configFile.Sections["PalMonConfig"];

                // Load PollInterval.
                options.PollInterval = config.PollInterval.Value;

                // Load LogPollInterval.
                options.LogPollInterval = config.LogPollInterval.Value;

                // Load ThreadInfoPollInterval.
                options.ThreadInfoPollInterval = config.ThreadInfoPollInterval.Value;
                options.DatabaseType = config.Database.Type;
                // store the result database details
                options.ResultDatabase = CreateDbConnectionInfo(config.Database);

                // Load thread monitoring configuration
                options.Processes = new System.Collections.Generic.List<string>();
                foreach (PalMon.Config.Process ProcessData in config.Processes)
                {
                    options.Processes.Add(ProcessData.Name);
                }

                // Load Cluster/Host configuration.
                var clusters = config.Clusters;
                foreach (Cluster cluster in clusters)
                {
                    var clusterName = cluster.Name;
                    foreach (Host host in cluster)
                    {
                        var resolvedHostname = HostnameHelper.Resolve(host.Name);
                        options.Hosts.Add(new Helpers.Host(resolvedHostname, clusterName));
                    }
                }


                // Load Log Folder configurations
                var logFolders = config.Logs;
                foreach (LogFolder logFolder in logFolders)
                {
                    PalMonOptions.LogFolderInfo logInfo = new PalMonOptions.LogFolderInfo();
                    logInfo.FolderToWatch = logFolder.Directory;
                    logInfo.DirectoryFilter = logFolder.Filter;
                    options.LogFolders.Add(logInfo);
                }

                var repoProps = config.TableauRepo;
                options.RepoHost = repoProps.Host;
                options.RepoPort = Convert.ToInt32(repoProps.Port);
                options.RepoUser = repoProps.Username;
                options.RepoPass = repoProps.Password;
                options.RepoDb = repoProps.Db;

            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Fatal(ex, "Error loading PalMon.config: {0})", ex);
                throw;
            }

            // Validate runtime options.
            if (!options.Valid())
            {
                Log.Fatal("Invalid options in configuration: " + options);
            }
            else
            {
                Log.Info("Successfully loaded PalMon config options! " + options);
            }
        }

        #endregion

        #region Private Methods

        private static IDbConnectionInfo CreateDbConnectionInfo(Database databaseConfig)
        {
            IDbConnectionInfo dbConnInfo = new DbConnectionInfo()
            {
                Server = databaseConfig.Server.Host,
                Port = databaseConfig.Server.Port,
                Username = databaseConfig.User.Login,
                Password = databaseConfig.User.Password,
                DatabaseName = databaseConfig.Name
            };



            if (!dbConnInfo.Valid())
            {
                throw new ConfigurationErrorsException("Missing required database connection information!");
            }

            return dbConnInfo;
        }

        #endregion
    }
}