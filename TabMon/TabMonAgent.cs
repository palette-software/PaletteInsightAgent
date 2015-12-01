﻿using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using TabMon.Config;
using TabMon.CounterConfig;
using TabMon.Counters;
using TabMon.Sampler;

using TabMon.LogPoller;
using TabMon.ThreadInfoPoller;

[assembly: CLSCompliant(true)]

namespace TabMon
{
    /// <summary>
    /// A timer-based performance monitoring agent.  Loads a set of counters from a config file and polls them periodically, passing the results to a writer object.
    /// </summary>
    public class TabMonAgent : IDisposable
    {
        private Timer timer;
        private Timer logPollTimer;
        private Timer threadInfoTimer;
        private Timer viewPathUpdateTimer;
        private LogPollerAgent logPollerAgent;
        private ThreadInfoAgent threadInfoAgent;
        private CounterSampler sampler;
        private ViewPathUpdaterAgent viewPathUpdater;
        private ITableauRepoConn tableauRepo;
        private readonly TabMonOptions options;
        private bool disposed;
        private const string PathToCountersConfig = @"Config\Counters.config";
        private const int WriteLockAcquisitionTimeout = 10; // In seconds.
        private static readonly object WriteLock = new object();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string Log4NetConfigKey = "log4net-config-file";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public TabMonAgent(bool loadOptionsFromConfig = true)
        {
            // "license check"
            if (DateTime.Now.Year > 2015)
            {
                Log.Fatal("License expired!");
                Environment.Exit(-1);
            }
            // Initialize log4net settings.
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyLocation));
            XmlConfigurator.Configure(new FileInfo(ConfigurationManager.AppSettings[Log4NetConfigKey]));

            // Load TabMonOptions.  In certain use cases we may not want to load options from the config, but provide them another way (such as via a UI).
            options = TabMonOptions.Instance;
            if (loadOptionsFromConfig)
            {
                TabMonConfigReader.LoadOptions();
            }

            Log.Info("Setting up LogPoller agent.");


            // Load the log poller config & start the agent
            //var logPollerConfig = LogPollerConfigurationLoader.load();
            logPollerAgent = new LogPollerAgent(options.FolderToWatch, options.DirectoryFilter,
                options.DbConnectionString);

            // start the thread info agent
            threadInfoAgent = new ThreadInfoAgent();

            // The tableau postgres repository
            if (ShouldUseRepo(options.RepoHost))
            {
                tableauRepo = new Tableau9RepoConn(options.RepoHost, options.RepoPort, options.RepoUser, options.RepoPass, options.RepoDb);
            }

            // The view path updater agent
            viewPathUpdater = new ViewPathUpdaterAgent(options.DbConnectionString);
        }

        ~TabMonAgent()
        {
            Dispose(false);
        }

        #region Public Methods

        /// <summary>
        /// Starts up the agent.
        /// </summary>
        public void Start()
        {
            Log.Info("Initializing TabMon..");

            // Assert that runtime options are valid.
            if (!TabMonOptions.Instance.Valid())
            {
                Log.Fatal("Invalid TabMon options specified!\nAborting..");
                return;
            }

            // Read Counters.config & create counters.
            Log.Info(String.Format(@"Loading performance counters from {0}\{1}..", Directory.GetCurrentDirectory(), PathToCountersConfig));
            ICollection<ICounter> counters;
            try
            {
                counters = CounterConfigLoader.Load(PathToCountersConfig, options.Hosts);
            }
            catch (ConfigurationErrorsException ex)
            {
                Log.Error(String.Format("Failed to correctly load '{0}': {1}\nAborting..", PathToCountersConfig, ex.Message));
                return;
            }
            Log.Debug(String.Format("Successfully loaded {0} {1} from configuration file.", counters.Count, "counter".Pluralize(counters.Count)));

            // Spin up counter sampler.
            sampler = new CounterSampler(counters, options.TableName);

            // Kick off the polling timer.
            Log.Info("TabMon initialized!  Starting performance counter polling..");
            timer = new Timer(callback: Poll, state: null, dueTime: 0, period: options.PollInterval * 1000);

            // Start the log poller agent
            logPollerAgent.start();
            logPollTimer = new Timer(callback: PollLogs, state: null, dueTime: 0, period: options.LogPollInterval * 1000);

            // Kick off the thread polling timer
            threadInfoTimer = new Timer(callback: PollThreadInfo, state: null, dueTime: 0, period: options.ThreadInfoPollInterval * 1000);

            // Start the view path updater timer
            viewPathUpdateTimer = new Timer(callback: UpdateViewPaths, state: null, dueTime: 0, period: options.LogPollInterval * 1000);
        }

        /// <summary>
        /// Stops the agent by disabling the timer.  Uses a write lock to prevent data from being corrupted mid-write.
        /// </summary>
        public void Stop()
        {
            Log.Info("Shutting down TabMon..");
            // Wait for write lock to finish before exiting to avoid corrupting data, up to a certain threshold.
            if (!Monitor.TryEnter(WriteLock, WriteLockAcquisitionTimeout * 1000))
            {
                Log.Error("Could not acquire write lock; forcing exit..");
            }
            else
            {
                Log.Debug("Acquired write lock gracefully..");
            }

            if (timer != null)
            {
                timer.Dispose();
            }

            // Stop the log poller agent
            if (logPollTimer != null)
            {
                Log.Info("Stopping logPollTimer...");
                logPollTimer.Dispose();
            }
            logPollerAgent.stop();

            // Stop the thread info timer
            if (threadInfoTimer != null)
            {
                Log.Info("Stopping thread info loop...");
                threadInfoTimer.Dispose();
            }

            if (viewPathUpdateTimer != null)
            {
                Log.Info("Stopping view path updater...");
                viewPathUpdateTimer.Dispose();
            }

            Log.Info("TabMon stopped.");
        }

        /// <summary>
        /// Indicates whether the agent is currently running (is initialized & has an active timer).
        /// </summary>
        /// <returns>Bool indicating whether the agent is currently running.</returns>
        public bool IsRunning()
        {
            return sampler != null && timer != null;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Polls the sampler's counters and writes the results to the writer object.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void Poll(object stateInfo)
        {
            var sampleResults = sampler.SampleAll();
            lock (WriteLock)
            {
                options.Writer.Write(sampleResults);
            }
        }


        /// <summary>
        /// Polls the logs from tableau and inserts them into the database
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollLogs(object stateInfo)
        {
            logPollerAgent.pollLogs(options.Writer, WriteLock);
        }

        /// <summary>
        /// Reads thread information from jmx and inserts them to the database 
        /// </summary>
        /// <param name="stateInfo"></param>
        private void PollThreadInfo(object stateInfo)
        {
            threadInfoAgent.poll(options.Writer, WriteLock);
        }


        private void UpdateViewPaths(object stateInfo)
        {
            viewPathUpdater.updateViewPath(tableauRepo);
        }

        private static bool ShouldUseRepo(string repoHost)
        {
            return !String.IsNullOrEmpty(repoHost);
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

            if (disposing)
            {
                if (options.Writer != null)
                {
                    options.Writer.Dispose();
                }
            }
            disposed = true;
        }

        #endregion IDisposable Methods
    }
}