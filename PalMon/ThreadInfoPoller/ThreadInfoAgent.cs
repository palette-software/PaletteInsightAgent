using DataTableWriter.Writers;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using PalMon.Counters;
using DataTableWriter.Helpers;

namespace PalMon.ThreadInfoPoller
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
        private long tableBaseId;
        public static readonly string InProgressLock = "Thread Info";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string HostName = Dns.GetHostName();

        public ThreadInfoAgent()
        {
            tableBaseId = Generator.CreteEpochPrefixedBaseId();
            Log.DebugFormat("Thread Info table base ID: {0}", tableBaseId);
        }

        public void poll(ICollection<string> processNames, IDataTableWriter writer, object WriteLock)
        {
            foreach (string processName in processNames)
            {
                var processList = Process.GetProcessesByName(processName);
                long threadInfoTableCount = 0;
                var threadInfoTable = ThreadTables.makeThreadInfoTable();
                foreach (var process in processList)
                {
                    pollThreadCountersOfProcess(process, threadInfoTable, ref threadInfoTableCount);
                }
                lock (WriteLock)
                {
                    if (threadInfoTableCount > 0)
                    {
                        try
                        {
                            writer.Write(threadInfoTable);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn(String.Format(@"Failed to write thread info table to DB! Exception message: {0}", ex.Message), ex);
                        }
                    }
                }
            }
        }

        protected void pollThreadCountersOfProcess(Process process, DataTable table, ref long serverLogsTableCount)
        {
            try
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    ThreadInfo threadInfo = new ThreadInfo();
                    threadInfo.processId = process.Id;
                    threadInfo.threadId = thread.Id;
                    threadInfo.cpuTime = thread.TotalProcessorTime.Ticks;
                    threadInfo.pollTimeStamp = DateTimeOffset.Now.UtcDateTime;
                    threadInfo.host = HostName;
                    threadInfo.instance = process.ProcessName;
                    ThreadTables.addToTable(table, threadInfo, ref tableBaseId);
                    serverLogsTableCount++;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format(@"Failed to poll thread info for process {0}! Exception message: {1}", process.ProcessName, ex.Message));
            }
        }
    }
}
