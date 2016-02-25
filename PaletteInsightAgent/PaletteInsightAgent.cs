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
        private LogPollerAgent logPollerAgent;
        private ThreadInfoAgent threadInfoAgent;
        private CounterSampler sampler;
        private readonly PaletteInsightAgentOptions options;
        private bool disposed;
        private const string PathToCountersYaml = @"Config\Counters.yml";
        private const int DBWriteLockAcquisitionTimeout = 10; // In seconds.
        private const int PollWaitTimeout = 1000;  // In milliseconds.
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const bool USE_COUNTERSAMPLES = false;
        private const bool USE_LOGPOLLER = false;
        private const bool USE_THREADINFO = false;

        private const bool USE_DB = false;
        private const bool USE_WEBSERVICE = true;

        private IOutput webserviceOutput;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public PaletteInsightAgent(bool loadOptionsFromConfig = true)
        {
            // Set the working directory
            Assembly assembly = Assembly.GetExecutingAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(assembly.Location));

            // Load PaletteInsightAgentOptions.  In certain use cases we may not want to load options from the config, but provide them another way (such as via a UI).
            options = PaletteInsightAgentOptions.Instance;
            PaletteInsight.Configuration.Loader.LoadConfigTo( LoadConfigFile("config/Config.yml"), options );

            // check the license after the configuration has been loaded.
            CheckLicense(Path.GetDirectoryName(assembly.Location) + "\\");

            // Showing the current version in the log
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            Log.Info("Palette Insight Agent version: " + version);

            if (USE_LOGPOLLER)
            {
                // Load the log poller config & start the agent
                logPollerAgent = new LogPollerAgent(options.LogFolders,
                    options.RepoHost, options.RepoPort, options.RepoUser, options.RepoPass, options.RepoDb);
            }

            if (USE_THREADINFO)
            {
                // start the thread info agent
                threadInfoAgent = new ThreadInfoAgent();
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

        private void CheckLicense(string pathToCheck)
        {
            try
            {
                Log.Info("Checking for licenses in: {0}", pathToCheck);
                // get the core count
                var coreCount = LicenseChecker.LicenseChecker.getCoreCount(
                    options.RepoHost,
                    options.RepoPort,
                    options.RepoUser,
                    options.RepoPass,
                    options.RepoDb
                    );
                // check for license.
                if (!LicenseChecker.LicenseChecker.checkForLicensesIn(pathToCheck, LicensePublicKey.PUBLIC_KEY, coreCount))
                {
                    Log.Fatal("No valid license found for Palette Insight in {0}. Exiting...", pathToCheck);
                    Environment.Exit(-1);
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Error during license check. Exception: {0}", e);
                Environment.Exit(-1);
            }
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

            if (USE_DB)
            {
                // Start the DB Writer
                IOutput output = new PostgresOutput(options.ResultDatabase);

                // On start try to send all unsent files
                DBWriter.TryToSendUnsentFiles(output);

                dbWriterTimer = new Timer(callback: WriteToDB, state: output, dueTime: 0, period: options.DBWriteInterval * 1000);
            }

            if (USE_WEBSERVICE)
            {
                var webserviceOutput = WebserviceOutput.MakeWebservice(
                        new WebserviceConfiguration{
                            Endpoint = "http://test:test@localhost:9000",
                            Username = "test",
                            Password = "test",
                        },
                        new BasicErrorHandler { }
                    );

                webserviceTimer = new Timer(callback: WriteToDB, state: webserviceOutput, dueTime: 0, period: 10 * 1000);
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

            // Wait for write lock to finish before exiting to avoid corrupting data, up to a certain threshold.
            if (!Monitor.TryEnter(DBWriter.DBWriteLock, DBWriteLockAcquisitionTimeout * 1000))
            {
                Log.Error("Could not acquire DB write lock; forcing exit..");
            }
            else
            {
                Log.Debug("Acquired DB write lock gracefully..");
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

            if (USE_WEBSERVICE) running = running && (webserviceTimer != null);
            if (USE_DB) running = running && (dbWriterTimer != null);
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
                CsvOutput.Write(sampleResults);
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

        private void WriteToDB(object stateInfo)
        {
            // Stateinfo contains an IOutput object
            DBWriter.Start((IOutput)stateInfo);
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