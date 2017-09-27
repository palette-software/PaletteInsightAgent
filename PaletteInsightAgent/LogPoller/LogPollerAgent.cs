using NLog;
using System;
using System.Collections.Generic;
using PaletteInsightAgent.Output;
using System.IO;

namespace PaletteInsightAgent.LogPoller
{

    class LogPollerAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string InProgressLock = "Log Poller";

        private ICollection<LogFileWatcher> watchers;
        private LogsToDbConverter logsToDbConverter;

        /// <summary>
        /// The maximum log lines in a batch for ChangeDelegate.
        /// </summary>
        private int logLinesPerBatch;


        private ICollection<PaletteInsightAgentOptions.LogFolderInfo> foldersToWatch;


        public LogPollerAgent(ICollection<PaletteInsightAgentOptions.LogFolderInfo> foldersToWatch, int logLinesPerBatch)
        {
            if (foldersToWatch.Count > 0)
            {
                Log.Info("Initializing LogPollerAgent with number of folders: {0}.", foldersToWatch.Count);
            }
            else
            {
                Log.Error("LogPollerAgent has no folder to watch!");
            }
            this.foldersToWatch = foldersToWatch;
            this.logLinesPerBatch = logLinesPerBatch;
            logsToDbConverter = new LogsToDbConverter();
        }

        // Start the log file watchers
        public void start()
        {

            watchers = new List<LogFileWatcher>();
            foreach (var folderInfo in foldersToWatch)
            {
                Log.Info("Starting LogFileWatcher in " + folderInfo.FolderToWatch + " with file mask:" + folderInfo.DirectoryFilter);
                watchers.Add(new LogFileWatcher(folderInfo.FolderToWatch, folderInfo.DirectoryFilter, logLinesPerBatch, folderInfo.LogFormat));
            }
        }

        // Stop the log file watchers
        public void stop()
        {
            Log.Info("Stopping LogFileWatcher.");
        }


        public void pollLogs()
        {
            // Keep track of the files we have already processed, so we wont have
            // doubles if a file matches multiple watchers
            var alreadySubmittedFiles = new HashSet<string>();

            foreach (var watcher in watchers)
            {
                watcher.watchChangeCycle((fullPath, lines, partCount) =>
                {
                    // we can get the same file in multiple parts, it is only a duplicate if it's
                    // full path and part index is the same
                    var fileId = String.Format("{0}|{1}", fullPath, partCount);

                    // check if we have already processed this file, and skip processing it it we have
                    if (alreadySubmittedFiles.Contains(fileId))
                    {
                        Log.Error("Skipping already processed change in '{0}'", fullPath);
                        return;

                    }

                    // add to the list of processed files
                    alreadySubmittedFiles.Add(fileId);

                    // create a new output table for the file
                    var serverLogsTable = LogTables.makeServerLogsTable(watcher.logFormat);

                    Log.Info("Got new {0} lines from {1}.", lines.Length, fullPath);

                    // process the newly found lines
                    logsToDbConverter.processServerLogLines(fullPath, lines, serverLogsTable);

                    // write the current batch out
                    WriteOutServerlogRows(serverLogsTable);
                }, () =>
                {
                    // if no change, just flush if needed
                    Log.Debug("No changes detected folder={0} filter={1} -- flushing if needed", watcher.watchedFolderPath, watcher.filter);
                });
            }

        }

        /// <summary>
        /// Helper that writes a serverlogs table to disk as a CSV
        /// </summary>
        /// <param name="serverLogsTable"></param>
        private static void WriteOutServerlogRows(System.Data.DataTable serverLogsTable)
        {
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
                OutputSerializer.Write(serverLogsTable, false);
            }

            Log.Info("Sent off {0}", statusLine);
        }
    }
}
