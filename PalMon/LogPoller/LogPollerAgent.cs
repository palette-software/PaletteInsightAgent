using NLog;
using System.Reflection;
using System;
using DataTableWriter.Writers;
using PalMon.Output;

namespace PalMon.LogPoller
{
    class LogPollResult
    {
        FilterStateChangeRow[] filterChangeRows;
    }


    class LogPollerAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string InProgressLock = "Log Poller";

        private LogFileWatcher watcher;
        private LogsToDbConverter logsToDbConverter;

        private ITableauRepoConn tableauRepo;

        private string folderToWatch;
        private string filter;



        public LogPollerAgent(string folderToWatch, string filterString, string repoHost, int repoPort, string repoUser, string repoPass, string repoDb)
        {
            Log.Info("Initializing LogPollerAgent with folder:" + folderToWatch + " and filter: " + filter);
            this.folderToWatch = folderToWatch;
            filter = filterString;
            logsToDbConverter = new LogsToDbConverter();
            // 
            tableauRepo = null;
            if (ShouldUseRepo(repoHost))
            {
                tableauRepo = new Tableau9RepoConn(repoHost, repoPort, repoUser, repoPass, repoDb);
            }
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


        public void pollLogs(CachingOutput output, object writeLock)
        {
            watcher.watchChangeCycle((filename, lines) =>
            {
                Log.Info("Got new {0} lines from {1}.", lines.Length, filename);
                logsToDbConverter.processServerLogLines(output, writeLock, filename, lines);
            }, () =>
            {
                // if no change, just flush if needed
                Log.Debug("No changes detected -- flushing if needed");
            });


            // whatever happened, we tick the output
            output.Tick();
        }

    }
}
