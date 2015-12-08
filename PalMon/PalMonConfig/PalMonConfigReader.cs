using DataTableWriter;
using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using DataTableWriter.Connection;
using DataTableWriter.Drivers;
using DataTableWriter.Writers;
using PalMon.Helpers;

namespace PalMon.Config
{
    /// <summary>
    /// Parses the main application config to initialize an instance of PalMonOptions.
    /// </summary>
    public static class PalMonConfigReader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                Log.Fatal(String.Format("Could not open configuration file: {0}", ex.Message));
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

                // Load OutputMode.
                var outputMode = config.OutputMode.Value;
                if (outputMode.Equals("DB", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.Writer = LoadDbWriterFromConfig(config);
                    options.TableName = config.Database.Table.Name;

                }
                else if (outputMode.Equals("CSV", StringComparison.InvariantCultureIgnoreCase))
                {
                    options.Writer = LoadCsvWriter();
                    options.TableName = "countersamples";
                }
                else
                {
                    Log.Fatal("Invalid output mode specified in configuration!");
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


                var pollerConfig = config.LogPoller;
                options.FolderToWatch = pollerConfig.Directory;
                options.DirectoryFilter = pollerConfig.Filter;

                var repoProps = config.TableauRepo;
                options.RepoHost = repoProps.Host;
                options.RepoPort = Convert.ToInt32(repoProps.Port);
                options.RepoUser = repoProps.Username;
                options.RepoPass = repoProps.Password;
                options.RepoDb = repoProps.Db;

            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Fatal(String.Format("Error loading PalMon.config: {0})", ex.Message));
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

        /// <summary>
        /// Initializes a new Database Writer object using a PalMonConfig.
        /// </summary>
        /// <param name="config">User's PalMon.config file containing the required Db Writer parameters.</param>
        /// <returns>Initialized DataTableDbWriter object.</returns>
        private static DataTableDbWriter LoadDbWriterFromConfig(PalMonConfig config)
        {
            DbDriverType dbDriverType;

            Log.Debug("Loading database configuration..");
            var databaseConfig = config.Database;

            var validDriverType = Enum.TryParse(databaseConfig.Type, true, out dbDriverType);
            if (!validDriverType)
            {
                throw new ConfigurationErrorsException("Invalid database driver type specified!");
            }

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

            var tableInitializationOptions = new DbTableInitializationOptions()
            {
                CreateTableDynamically = true,
                UpdateDbTableToMatchSchema = true,
                UpdateSchemaToMatchDbTable = true
            };

            Log.Info("Connecting to results database..");
            try
            {
                return new DataTableDbWriter(dbDriverType, dbConnInfo, tableInitializationOptions);
            }
            catch (Exception ex)
            {
                Log.Fatal("Could not initialize writer: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Initializes a new CSV Writer object.
        /// </summary>
        /// <returns>Initialized DataTableCSVWriter object.</returns>
        private static DataTableCsvWriter LoadCsvWriter()
        {
            Log.Info("Initializing CSV writer..");
            // Set up output directory & filename.
            var resultOutputDirectory = Directory.GetCurrentDirectory() + @"\Results";
            var csvFileName = String.Format("PalMonResult_{0}.csv", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            var csvFilePath = resultOutputDirectory + @"\" + csvFileName;

            try
            {
                Directory.CreateDirectory(resultOutputDirectory);
                return new DataTableCsvWriter(csvFilePath);
            }
            catch (Exception ex)
            {
                Log.Fatal(String.Format("Could not open file stream to {0}: {1}", csvFileName, ex.Message));
                return null;
            }
        }

        #endregion
    }
}