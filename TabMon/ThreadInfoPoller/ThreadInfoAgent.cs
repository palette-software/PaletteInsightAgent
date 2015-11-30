using DataTableWriter.Writers;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using TabMon.Counters;

namespace TabMon.ThreadInfoPoller
{
    struct ThreadInfo
    {
        public string host;
        public string instance;
        public long processId;
        public long threadId;
        public long cpuTime;
        public DateTime pollTimeStamp;
    }

    class ThreadInfoAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string HostName = Dns.GetHostName();

        public void poll(IDataTableWriter writer, object WriteLock)
        {
            const string processName = "vizqlserver";
            var processList = Process.GetProcessesByName(processName);
            long serverLogsTableCount = 0;
            var serverLogsTable = ThreadTables.makeThreadInfoTable();
            foreach (var process in processList)
            {
                pollThreadsOfCounter(process,  serverLogsTable, ref serverLogsTableCount);
            }
            lock (WriteLock)
            {
                if (serverLogsTableCount > 0) writer.Write(serverLogsTable);
            }
        }

        protected void pollThreadsOfCounter(Process process, DataTable table, ref long serverLogsTableCount)
        {
            try
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    ThreadInfo threadInfo = new ThreadInfo();
                    threadInfo.processId = process.Id;
                    threadInfo.threadId = thread.Id;
                    threadInfo.cpuTime = thread.TotalProcessorTime.Ticks;
                    threadInfo.pollTimeStamp = DateTime.Now;
                    threadInfo.host = HostName;
                    threadInfo.instance = process.ProcessName;
                    ThreadTables.addToTable(table, threadInfo);
                    serverLogsTableCount++;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format(@"Failed to poll thread info for process {0}. Exception message: {1}", process.ProcessName, ex.Message));
            }
        }
    }
}
