using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using PaletteInsightAgent.CounterConfig;
using PaletteInsightAgent.Counters;
using PaletteInsightAgent.Sampler;

using PaletteInsightAgent.LogPoller;
using PaletteInsightAgent.ThreadInfoPoller;
using PaletteInsightAgent.Output;
using System.Diagnostics;
using PaletteInsight.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PaletteInsightAgent.Output.OutputDrivers;
using PaletteInsightAgent.RepoTablesPoller;
using PaletteInsightAgent.Helpers;

[assembly: CLSCompliant(true)]

namespace PaletteInsightAgent
{
    /// <summary>
    /// A timer-based performance monitoring agent.  Loads a set of counters from a config file and polls them periodically, passing the results to a writer object.
    /// </summary>
    public class PaletteInsightAgent : IDisposable
    {
        private Timer timer;
        private Timer logPollTimer;
        private Timer threadInfoTimer;
        private Timer dbWriterTimer;
        private Timer webserviceTimer;
        private Timer repoTablesPollTimer;
        private Timer streamingTablesPollTimer;
        private LogPollerAgent logPollerAgent;
        private ThreadInfoAgent threadInfoAgent;
        private RepoPollAgent repoPollAgent;
        private CounterSampler sampler;
        private ITableauRepoConn tableauRepo;
        private IOutput output;
        private readonly PaletteInsightAgentOptions options;
        private bool disposed;
        private const string PathToCountersYaml = @"Config\Counters.yml";
        private const int DBWriteLockAcquisitionTimeout = 10; // In seconds.
        private const int PollWaitTimeout = 1000;  // In milliseconds.
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private bool USE_COUNTERSAMPLES = true;
        private bool USE_LOGPOLLER = true;
        private bool USE_THREADINFO = true;

        // use the constant naming convention for now as the mutability
        // of this variable is temporary until the Db output is removed
        private bool USE_TABLEAU_REPO = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public PaletteInsightAgent(bool loadOptionsFromConfig = true)
        {
            // Set the working directory
            Assembly assembly = Assembly.GetExecutingAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(assembly.Location));

            // Load PaletteInsightAgentOptions.  In certain use cases we may not want to load options from the config, but provide them another way (such as via a UI).
            options = PaletteInsightAgentOptions.Instance;
            PaletteInsight.Configuration.Loader.LoadConfigTo(LoadConfigFile("config/Config.yml"), options);

            tableauRepo = new Tableau9RepoConn(options.RepositoryDatabase);

            // check the license after the configuration has been loaded.
            var license = CheckLicense(Path.GetDirectoryName(assembly.Location) + "\\");

            // Make sure that our HTTP client is initialized, because Splunk logger might be enabled
            // and it is using HTTP to send log messages to Splunk.
            APIClient.Init(options.WebserviceConfig);

            // Add the webservice username/auth token from the license
            PaletteInsight.Configuration.Loader.updateWebserviceConfigFromLicense(options, license);

            // Showing the current version in the log
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            Log.Info("Palette Insight Agent version: " + version);

            USE_LOGPOLLER = options.UseLogPolling;
            USE_THREADINFO = options.UseThreadInfo;
            USE_COUNTERSAMPLES = options.UseCounterSamples;
            USE_TABLEAU_REPO = options.UseRepoPolling;

            if (USE_LOGPOLLER)
            {
                // Load the log poller config & start the agent
                logPollerAgent = new LogPollerAgent(options.LogFolders, options.LogLinesPerBatch);
            }

            if (USE_THREADINFO)
            {
                // start the thread info agent
                threadInfoAgent = new ThreadInfoAgent();
            }

            if (USE_TABLEAU_REPO)
            {
                repoPollAgent = new RepoPollAgent();
            }
        }

        private PaletteInsightConfiguration LoadConfigFile(string filename)
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

        private Licensing.License CheckLicense(string pathToCheck)
        {
            try
            {
                Log.Info("Checking for licenses in: {0}", pathToCheck);
                var coreCount = tableauRepo.getCoreCount();
                var license = LicenseChecker.LicenseChecker.checkForLicensesIn(pathToCheck, LicensePublicKey.PUBLIC_KEY, coreCount);
                // check for license.
                if (license == null)
                {
                    Log.Fatal("No valid license found for Palette Insight in {0}. Exiting...", pathToCheck);
                    Environment.Exit(-1);
                }

                return license;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Error during license check. Exception: {0}", e);
                Environment.Exit(-1);
            }
            return null;
        }

        ~PaletteInsightAgent()
        {
            Dispose(false);
        }

        #region Public Methods

        /// <summary>
        /// Starts up the agent.
        /// </summary>
        public void Start()
        {
            Log.Info("Initializing PaletteInsightAgent..");

            // Assert that runtime options are valid.
            if (!PaletteInsightAgentOptions.Instance.Valid())
            {
                Log.Fatal("Invalid PaletteInsightAgent options specified!\nAborting..");
                return;
            }

            // only start the JMX if we want to
            if (USE_COUNTERSAMPLES)
            {
                ICollection<ICounter> counters;
                try
                {
                    counters = CounterConfigLoader.Load(PathToCountersYaml);
                }
                catch (ConfigurationErrorsException ex)
                {
                    Log.Error("Failed to correctly load '{0}': {1}\nAborting..", PathToCountersYaml, ex.Message);
                    return;
                }

                // Spin up counter sampler.
                sampler = new CounterSampler(counters);

                // Kick off the polling timer.
                Log.Info("PaletteInsightAgent initialized!  Starting performance counter polling..");
                timer = new Timer(callback: Poll, state: null, dueTime: 0, period: options.PollInterval * 1000);
            }


            if (USE_LOGPOLLER)
            {
                // Start the log poller agent
                logPollerAgent.start();
                logPollTimer = new Timer(callback: PollLogs, state: null, dueTime: 0, period: options.LogPollInterval * 1000);
            }

            if (USE_THREADINFO)
            {
                // Kick off the thread polling timer
                threadInfoTimer = new Timer(callback: PollThreadInfo, state: null, dueTime: 0, period: options.ThreadInfoPollInterval * 1000);
            }

            // send the metadata if there is a tableau repo behind us
            if (USE_TABLEAU_REPO)
            {

                // On start get the schema of the repository tables
                var table = tableauRepo.GetSchemaTable();

                // Add the metadata of the agent table to the schema table
                DataTableUtils.AddAgentMetadata(table);

                // Serialize schema table so that it gets uploaded with all other tables
                OutputSerializer.Write(table);

                // Do the same for index data
                table = tableauRepo.GetIndices();
                OutputSerializer.Write(table);

            }

            output = WebserviceOutput.MakeWebservice(options.WebserviceConfig);
            webserviceTimer = new Timer(callback: UploadData, state: output, dueTime: 0, period: options.UploadInterval * 1000);

            if (USE_TABLEAU_REPO)
            {
                // Poll Tableau repository data as well
                repoTablesPollTimer = new Timer(callback: PollFullTables, state: output, dueTime: 0, period: options.RepoTablesPollInterval * 1000);
                streamingTablesPollTimer = new Timer(callback: PollStreamingTables, state: output, dueTime: 0, period: options.RepoTablesPollInterval * 1000);
            }

        }


        /// <summary>
        /// Stops the agent by disabling the timer.  Uses a write lock to prevent data from being corrupted mid-write.
        /// </summary>
        public void Stop()
        {
            Log.Info("Shutting down PaletteInsightAgent..");

            if (USE_COUNTERSAMPLES)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }

            if (USE_LOGPOLLER)
            {
                // Stop the log poller agent
                if (logPollTimer != null)
                {
                    logPollTimer.Dispose();
                    Log.Info("Stopping logPollTimer.");
                }
                logPollerAgent.stop();
            }

            if (USE_THREADINFO)
            {
                // Stop the thread info timer
                if (threadInfoTimer != null)
                {
                    threadInfoTimer.Dispose();
                }
            }

            if (dbWriterTimer != null)
            {
                dbWriterTimer.Dispose();
            }

            if (webserviceTimer != null)
            {
                webserviceTimer.Dispose();
            }

            if (streamingTablesPollTimer != null)
            {
                streamingTablesPollTimer.Dispose();
            }

            if (repoTablesPollTimer != null)
            {
                repoTablesPollTimer.Dispose();
            }

            Log.Info("PaletteInsightAgent stopped.");
        }

        /// <summary>
        /// Indicates whether the agent is currently running (is initialized & has an active timer).
        /// </summary>
        /// <returns>Bool indicating whether the agent is currently running.</returns>
        public bool IsRunning()
        {
            var running = true;
            if (USE_COUNTERSAMPLES) running = running && (sampler != null && timer != null);
            if (USE_LOGPOLLER) running = running && (logPollTimer != null);
            if (USE_THREADINFO) running = running && (threadInfoTimer != null);
            running = running && (webserviceTimer != null);
            if (USE_TABLEAU_REPO) running = running && (repoTablesPollTimer != null && streamingTablesPollTimer != null);
            return running;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Polls the sampler's counters and writes the results to the writer object.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void Poll(object stateInfo)
        {
            tryStartIndividualPoll(CounterSampler.InProgressLock, PollWaitTimeout, () =>
            {
                var sampleResults = sampler.SampleAll();
                OutputSerializer.Write(sampleResults);
            });
        }


        /// <summary>
        /// Polls the logs from tableau and inserts them into the database
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollLogs(object stateInfo)
        {
            tryStartIndividualPoll(LogPollerAgent.InProgressLock, PollWaitTimeout, () =>
            {
                logPollerAgent.pollLogs();
            });
        }

        /// <summary>
        /// Reads thread information from jmx and inserts them to the database 
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollThreadInfo(object stateInfo)
        {
            tryStartIndividualPoll(ThreadInfoAgent.InProgressLock, PollWaitTimeout, () =>
            {
                Log.Info("Polling threadinfo");
                threadInfoAgent.poll(options.Processes, options.AllProcesses);
            });
        }

        /// <summary>
        /// Get Tableau repository tables from the database
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollFullTables(object stateInfo)
        {
            tryStartIndividualPoll(RepoPollAgent.FullTablesInProgressLock, PollWaitTimeout, () =>
            {
                Log.Info("Polling Repostoriy tables");
                repoPollAgent.PollFullTables(tableauRepo, options.RepositoryTables);
            });
        }

        /// <summary>
        /// Get Tableau repository streaming tables from the database
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollStreamingTables(object stateInfo)
        {
            tryStartIndividualPoll(RepoPollAgent.StreamingTablesInProgressLock, PollWaitTimeout, () =>
            {
                Log.Info("Polling streaming tables");
                repoPollAgent.PollStreamingTables(tableauRepo, options.RepositoryTables, (IOutput)stateInfo);
            });
        }


        private void UploadData(object stateInfo)
        {
            var thread = new Thread(() =>
            {
                if (!Monitor.TryEnter(FileUploader.FileUploadLock, FileUploader.fileUploadLockTimeout))
                {
                    Log.Debug("Skipping file upload as it is already in progress.");
                    return;
                }

                try
                {
                    FileUploader.Start((IOutput)stateInfo, options.ProcessedFilestTTL, options.StorageLimit);
                }
                finally
                {
                    Monitor.Exit(FileUploader.FileUploadLock);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Checks whether polling is in progress at the moment for a given poll method.
        /// If not, it executes the poll.
        /// </summary>
        private void tryStartIndividualPoll(object pollTypeLock, int timeout, Action pollDelegate)
        {
            if (!Monitor.TryEnter(pollTypeLock, timeout))
            {
                // Do not execute the poll delegate as it is already being executed.
                Log.Debug("Skipping poll as it is already in progress: " + pollTypeLock.ToString());
                return;
            }

            try
            {
                pollDelegate();
            }
            catch(Exception e)
            {
                Log.Error(e, "Exception during poll:{0}", e);
            }
            finally
            {
                // Ensure that the lock is released.
                Monitor.Exit(pollTypeLock);
            }
        }

        #endregion Private Methods

        #region IDisposable Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            disposed = true;
        }

        #endregion IDisposable Methods
    }
}