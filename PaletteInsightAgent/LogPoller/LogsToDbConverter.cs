using NLog;
using System.Reflection;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Net;

using PaletteInsightAgent.Output;
using System.IO;

namespace PaletteInsightAgent.LogPoller
{
    class LogsToDbConverter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string HostName { get; set; }

        private const string GROUP_FILTER_RX = @"<groupfilter function='member' level='(.*?)' member='(.*?)'.*?/>";

        /// <summary>
        /// Creates a new instance of LogsToDbConverter.
        /// </summary>
        /// <param name="writer">The DataTable Writer to use.</param>
        public LogsToDbConverter()
        {
            this.HostName = Dns.GetHostName();
        }

        /// <summary>
        /// This function insert a row into the Serverlogs table in PaletteInsightAgent DB
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="jsonStringLines"></param>
        /// <param name="serverLogsTable"></param>
        public void processServerLogLines(String fullPath, String[] jsonStringLines, DataTable serverLogsTable)
        {
            try
            {
                addServerLogs(fullPath, jsonStringLines, serverLogsTable);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while adding to server logs. Exception:");
            }
        }

        private void addServerLogs(string fullPath, string[] logLines, DataTable serverLogsTable)
        {
            // get the base filename for logging
            var filename = Path.GetFileName(fullPath);
            // log that we have started
            foreach (var logLine in logLines)
            {
                insertIntoServerLogsTable(filename, serverLogsTable, logLine);
            }
        }

        private void insertIntoServerLogsTable(string filename, DataTable serverLogsTable, string logLine)
        {
            // Add the new row to the table
            var row = serverLogsTable.NewRow();

            row["filename"] = filename;
            row["host_name"] = HostName;
            row["line"] = logLine;
            serverLogsTable.Rows.Add(row);
        }
    }
}
