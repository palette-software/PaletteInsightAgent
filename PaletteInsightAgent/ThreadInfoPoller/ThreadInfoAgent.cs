using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using PaletteInsightAgent.Counters;
using PaletteInsightAgent.Output;
using PaletteInsight.Configuration;

namespace PaletteInsightAgent.ThreadInfoPoller
{
    struct ThreadInfo
    {
        public string host;
        public string process;
        public long processId;
        public long threadId;
        public long cpuTime;
        public long workingSet;
        public int threadCount;
        public DateTime pollTimeStamp;
        public DateTime pollCycleTimeStamp;
        public DateTime startTimeStamp;
    }

    class ThreadInfoAgent
    {
        public static readonly string InProgressLock = "Thread Info";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string HostName = Dns.GetHostName();

        public void poll(IDictionary<string, ProcessData> processData, bool allProcesses)
        {
            var threadInfoTable = ThreadTables.makeThreadInfoTable();
            long threadInfoTableCount = 0;
            ICollection<Process> processList;

            if (allProcesses)
            {
                processList = Process.GetProcesses();
            }
            else
            {
                processList = new List<Process>();
                foreach (string processName in processData.Keys)
                {
                    processList.AddRange(Process.GetProcessesByName(processName));
                }
            }
            pollProcessList(processList, processData, threadInfoTable, ref threadInfoTableCount);


            if (threadInfoTableCount > 0)
            {
                OutputSerializer.Write(threadInfoTable);
            }
        }

        protected void pollProcessList(ICollection<Process> processList, IDictionary<string, ProcessData> processData, DataTable threadInfoTable, ref long threadInfoTableCount)
        {
            foreach (var process in processList)
            {
                var threadLevel = processData.ContainsKey(process.ProcessName) && processData[process.ProcessName].Granularity == "Thread";
                pollThreadCountersOfProcess(process, threadLevel, threadInfoTable, ref threadInfoTableCount);
            }
        }

        protected void addInfoToTable(Process process, DataTable table, int threadId, long ticks, DateTime pollCycleTimeStamp, 
                                        DateTime startTimeStamp)
        {
            ThreadInfo threadInfo = new ThreadInfo();
            threadInfo.processId = process.Id;
            threadInfo.threadId = threadId;
            threadInfo.cpuTime = ticks;
            threadInfo.pollTimeStamp = DateTimeOffset.Now.UtcDateTime;
            threadInfo.pollCycleTimeStamp = pollCycleTimeStamp;
            threadInfo.startTimeStamp = startTimeStamp;
            threadInfo.host = HostName;
            threadInfo.process = process.ProcessName;

            // When on process level add threadCount as well
            if (threadId == -1)
            {
                threadInfo.threadCount = process.Threads.Count;
                threadInfo.workingSet = process.WorkingSet64;
            }
            ThreadTables.addToTable(table, threadInfo);
        }

        protected void pollThreadCountersOfProcess(Process process, bool threadLevel, DataTable table, ref long threadInfoTableCount)
        {
            try
            {
                // Store the total processor time of the whole process so that we can do sanity checks on the sum of thread cpu times
                var pollCycleTimeStamp = DateTimeOffset.Now.UtcDateTime;
                addInfoToTable(process, table, -1, process.TotalProcessorTime.Ticks, pollCycleTimeStamp, process.StartTime.ToUniversalTime());
                threadInfoTableCount++;

                if (threadLevel)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        try
                        {
                            addInfoToTable(process, table, thread.Id, thread.TotalProcessorTime.Ticks, pollCycleTimeStamp, thread.StartTime.ToUniversalTime());
                            threadInfoTableCount++;
                        }
                        catch (InvalidOperationException)
                        {
                            // This can happen when a thread exits while we try to get info from it. It is normal operation so nothing to do here.
                            continue;
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // This can happen when a process exits while we try to get info from it. It is normal operation so nothing to do here.
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // This happens when we get acccess denied for a process. That's normal so nothing to do here
            }
            catch (Exception ex)
            {
                Log.Error("Failed to poll thread info for process {0}! Exception message: {1}", process.ProcessName, ex.Message);
            }
        }
    }
}
