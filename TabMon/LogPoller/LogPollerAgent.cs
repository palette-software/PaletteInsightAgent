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

        private string folderToWatch;
        private string filter;



        public LogPollerAgent(string folderToWatch, string filterString)
        {
            Log.Info("Initializing LogPollerAgent with folder:" + folderToWatch + " and filter: " + filter);
            this.folderToWatch = folderToWatch;
            filter = filterString;
            logsToDbConverter = new LogsToDbConverter();
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
            });
        }
    }
}
