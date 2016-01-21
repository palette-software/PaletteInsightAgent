using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using PalMon.Counters;
using PalMon.Output;

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
        public static readonly string InProgressLock = "Thread Info";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string HostName = Dns.GetHostName();

        public void poll(ICollection<string> processNames, CachingOutput writer, object WriteLock)
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
                            Log.Warn("Failed to write thread info table to DB! Exception message: {0}", ex.Message);
                        }
                    }
                }
            }
        }

        protected void addInfoToTable(Process process, DataTable table, int threadId, long ticks)
        {
            ThreadInfo threadInfo = new ThreadInfo();
            threadInfo.processId = process.Id;
            threadInfo.threadId = threadId;
            threadInfo.cpuTime = ticks;
            threadInfo.pollTimeStamp = DateTimeOffset.Now.UtcDateTime;
            threadInfo.host = HostName;
            threadInfo.instance = process.ProcessName;
            ThreadTables.addToTable(table, threadInfo);
        }

        protected void pollThreadCountersOfProcess(Process process, DataTable table, ref long serverLogsTableCount)
        {
            try
            {
                addInfoToTable(process, table, -1, process.TotalProcessorTime.Ticks);
                serverLogsTableCount++;
                foreach (ProcessThread thread in process.Threads)
                {
                    addInfoToTable(process, table, thread.Id, thread.TotalProcessorTime.Ticks);
                    serverLogsTableCount++;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to poll thread info for process {0}! Exception message: {1}", process.ProcessName, ex.Message);
            }
        }
    }
}
