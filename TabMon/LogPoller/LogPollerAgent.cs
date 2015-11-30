using log4net;
using System.Reflection;
using System;
using System.Data;
using DataTableWriter.Writers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
{
    class LogPollerAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private LogFileWatcher watcher;
        private LogsToDbConverter logsToDbConverter;

        private ITableauRepoConn tableauRepo;

        /// <summary>
        /// The delayed updater for the view paths.
        /// </summary>
        private IViewPathUpdater viewPathUpdater;

        private string folderToWatch;
        private string filter;



        public LogPollerAgent(string folderToWatch, string filterString, string repoHost, int repoPort, string repoUser, string repoPass, string repoDb, string dbConnectionString)
        {
            Log.Info("Initializing LogPollerAgent with folder:" + folderToWatch + " and filter: " + filter);
            this.folderToWatch = folderToWatch;
            filter = filterString;
            logsToDbConverter = new LogsToDbConverter();
            // create the repo 
            tableauRepo = null;
            if (ShouldUseRepo(repoHost))
            {
                tableauRepo = new Tableau9RepoConn(repoHost, repoPort, repoUser, repoPass, repoDb);
            }
            // Create the view path updater
            viewPathUpdater = new PostgresViewPathUpdater(dbConnectionString);
        }

        private static bool ShouldUseRepo(string repoHost)
        {
            return !String.IsNullOrEmpty(repoHost);
        }


        // Start the log file watcher
        public void start()
        {

            Log.Info("Starting LogFileWatcher in " + folderToWatch + " with file mask:" + filter);
            watcher = new LogFileWatcher(folderToWatch, filter);
        }

        // Stop the log file watcher
        public void stop()
        {
            Log.Info("Stopping LogFileWatcher.");
        }


        /// <summary>
        /// Actual function to poll from the logs
        /// </summary>
        /// <returns></returns>
        public void pollLogs(IDataTableWriter writer, object writeLock)
        {
            watcher.watchChangeCycle((string filename, string[] lines) =>
            {
                Log.Info("Got new " + lines.Length + " lines from " + filename );
                logsToDbConverter.processServerLogLines(writer, writeLock, filename, lines);
                viewPathUpdater.updateViewPaths(tableauRepo);
            });
        }
    }
}
