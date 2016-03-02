using NLog;
using System;
using System.Collections.Generic;
using PaletteInsightAgent.Output;
using PaletteInsightAgent.RepoTablesPoller;

namespace PaletteInsightAgent.LogPoller
{

    class LogPollerAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string InProgressLock = "Log Poller";

        private ICollection<LogFileWatcher> watchers;
        private LogsToDbConverter logsToDbConverter;


        private ICollection<PaletteInsightAgentOptions.LogFolderInfo> foldersToWatch;


        public LogPollerAgent(ICollection<PaletteInsightAgentOptions.LogFolderInfo> foldersToWatch, IDbConnectionInfo repositoryDB)
        {
            Log.Info("Initializing LogPollerAgent with number of folders: {0}.", foldersToWatch.Count);
            this.foldersToWatch = foldersToWatch;
            logsToDbConverter = new LogsToDbConverter();
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

            foreach (var watcher in watchers)
            { 
                watcher.watchChangeCycle((filename, lines) =>
                {
                    Log.Info("Got new {0} lines from {1}.", lines.Length, filename);
                    logsToDbConverter.processServerLogLines(filename, lines, serverLogsTable);
                }, () =>
                {
                    // if no change, just flush if needed
                    Log.Debug("No changes detected folder={0} filter={1} -- flushing if needed", watcher.watchedFolderPath, watcher.filter);
                });
            }

            var serverLogsTableCount = serverLogsTable.Rows.Count;

            if (serverLogsTableCount == 0)
            {
                // There is nothing collected.
                return;
            }

            var statusLine = String.Format("{0} server log {1}",
                 serverLogsTableCount, "row".Pluralize(serverLogsTableCount));


            Log.Info("Sending off " + statusLine);


            if (serverLogsTableCount > 0)
            {
                OutputSerializer.Write(serverLogsTable);
            }

            Log.Info("Sent off {0}", statusLine);
        }

    }
}
