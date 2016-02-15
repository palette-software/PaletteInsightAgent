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


        public void pollLogs()
        {
            var serverLogsTable = LogTables.makeServerLogsTable();
            var filterStateTable = LogTables.makeFilterStateAuditTable();

            foreach (var watcher in watchers)
            { 
                watcher.watchChangeCycle((filename, lines) =>
                {
                    Log.Info("Got new {0} lines from {1}.", lines.Length, filename);
                    logsToDbConverter.processServerLogLines(filename, lines, serverLogsTable, filterStateTable);
                }, () =>
                {
                    // if no change, just flush if needed
                    Log.Debug("No changes detected folder={0} filter={1} -- flushing if needed", watcher.watchedFolderPath, watcher.filter);
                });
            }

            var filterStateCount = filterStateTable.Rows.Count;
            var serverLogsTableCount = serverLogsTable.Rows.Count;

            if (filterStateCount == 0 && serverLogsTableCount == 0)
            {
                // There is nothing collected.
                return;
            }

            var statusLine = String.Format("{0} filter {1} and {2} server log {3}",
                filterStateCount, "row".Pluralize(filterStateCount),
                 serverLogsTableCount, "row".Pluralize(serverLogsTableCount));


            Log.Info("Sending off " + statusLine);


            if (filterStateCount > 0)
            {
                CsvOutput.Write(filterStateTable);
            }

            if (serverLogsTableCount > 0)
            {
                CsvOutput.Write(serverLogsTable);
            }

            Log.Info("Sent off {0}", statusLine);
        }

    }
}
