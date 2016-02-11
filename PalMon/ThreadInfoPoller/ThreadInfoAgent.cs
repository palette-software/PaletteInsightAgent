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
        public DateTime pollCycleTimeStamp;
        public DateTime startTimeStamp;
    }

    class ThreadInfoAgent
    {
        public static readonly string InProgressLock = "Thread Info";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string HostName = Dns.GetHostName();

        public void poll(ICollection<string> processNames, object WriteLock)
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
                            CsvOutput.Write(threadInfoTable);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Failed to write thread info table to DB! Exception message: {0}", ex.Message);
                        }
                    }
                }
            }
        }

        protected void addInfoToTable(Process process, DataTable table, int threadId, long ticks, DateTime pollCycleTimeStamp, DateTime startTimeStamp)
        {
            ThreadInfo threadInfo = new ThreadInfo();
            threadInfo.processId = process.Id;
            threadInfo.threadId = threadId;
            threadInfo.cpuTime = ticks;
            threadInfo.pollTimeStamp = DateTimeOffset.Now.UtcDateTime;
            threadInfo.pollCycleTimeStamp = pollCycleTimeStamp;
            threadInfo.startTimeStamp = startTimeStamp;
            threadInfo.host = HostName;
            threadInfo.instance = process.ProcessName;
            ThreadTables.addToTable(table, threadInfo);
        }

        protected void pollThreadCountersOfProcess(Process process, DataTable table, ref long serverLogsTableCount)
        {
            try
            {
                // Store the total processor time of the whole process so that we can do sanity checks on the sum of thread cpu times
                var pollCycleTimeStamp = DateTimeOffset.Now.UtcDateTime;
                addInfoToTable(process, table, -1, process.TotalProcessorTime.Ticks, pollCycleTimeStamp, process.StartTime.ToUniversalTime());
                serverLogsTableCount++;

                foreach (ProcessThread thread in process.Threads)
                {
                    try
                    {
                        addInfoToTable(process, table, thread.Id, thread.TotalProcessorTime.Ticks, pollCycleTimeStamp, thread.StartTime.ToUniversalTime());
                        serverLogsTableCount++;
                    }
                    catch (InvalidOperationException)
                    {
                        // This can happen when a thread exits while we try to get info from it. It is normal operation so nothing to do here.
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to poll thread info for process {0}! Exception message: {1}", process.ProcessName, ex.Message);
            }
        }
    }
}
