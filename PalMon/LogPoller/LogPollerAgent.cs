using NLog;
using System.Reflection;
using System;
using System.Collections.Generic;
using PalMon.Output;

namespace PalMon.LogPoller
{

    class LogPollerAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string InProgressLock = "Log Poller";

        private ICollection<LogFileWatcher> watchers;
        private LogsToDbConverter logsToDbConverter;

        private ITableauRepoConn tableauRepo;

        private ICollection<PalMonOptions.LogFolderInfo> foldersToWatch;


        public LogPollerAgent(ICollection<PalMonOptions.LogFolderInfo> foldersToWatch, string repoHost, int repoPort, string repoUser, string repoPass, string repoDb)
        {
            Log.Info("Initializing LogPollerAgent with number of folders: {0}.", foldersToWatch.Count);
            this.foldersToWatch = foldersToWatch;
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


        // Start the log file watchers
        public void start()
        {

            watchers = new List<LogFileWatcher>();
            foreach (var folderInfo in foldersToWatch)
            {
                Log.Info("Starting LogFileWatcher in " + folderInfo.FolderToWatch + " with file mask:" + folderInfo.DirectoryFilter);
                watchers.Add(new LogFileWatcher(folderInfo.FolderToWatch, folderInfo.DirectoryFilter));
            }
        }

        // Stop the log file watchers
        public void stop()
        {
            Log.Info("Stopping LogFileWatcher.");
        }


        public void pollLogs(CachingOutput output, object writeLock)
        {
            foreach (var watcher in watchers)
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
			}
        }

    }
}
