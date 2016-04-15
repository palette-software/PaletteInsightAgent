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
using System.Threading;

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
        public bool threadLevel;
    }

    class ThreadInfoAgent
    {
        public static readonly string InProgressLock = "Thread Info";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string HostName = Dns.GetHostName();

        private int pollInterval;
        private DateTime previousPollCycleTimeStamp;

        public ThreadInfoAgent(int pollInterval)
        {
            this.pollInterval = pollInterval;
        }

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
            var pollCycleTimeStamp = CalculatePollCycleTimeStamp();

            foreach (var process in processList)
            {
                var threadLevel = processData.ContainsKey(process.ProcessName) && processData[process.ProcessName].Granularity == "Thread";
                pollThreadCountersOfProcess(process, threadLevel, threadInfoTable, ref threadInfoTableCount, pollCycleTimeStamp);
            }
        }

        protected void addInfoToTable(Process process, DataTable table, int threadId, long ticks, DateTime pollCycleTimeStamp, 
                                        DateTime startTimeStamp, bool threadLevel)
        {
            ThreadInfo threadInfo = new ThreadInfo();
            threadInfo.processId = process.Id;
            threadInfo.threadId = threadId;
            threadInfo.cpuTime = ticks;
            threadInfo.pollTimeStamp = DateTime.UtcNow;
            threadInfo.pollCycleTimeStamp = pollCycleTimeStamp;
            threadInfo.startTimeStamp = startTimeStamp;
            threadInfo.host = HostName;
            threadInfo.threadLevel = threadLevel;
            threadInfo.process = process.ProcessName;

            // When on process level add threadCount as well
            if (threadId == -1)
            {
                threadInfo.threadCount = process.Threads.Count;
                threadInfo.workingSet = process.WorkingSet64;
            }
            ThreadTables.addToTable(table, threadInfo);
        }

        protected void pollThreadCountersOfProcess(Process process, bool threadLevel, DataTable table, ref long threadInfoTableCount, DateTime pollCycleTimeStamp)
        {
            try
            {
                // Store the total processor time of the whole process so that we can do sanity checks on the sum of thread cpu times
                addInfoToTable(process, table, -1, process.TotalProcessorTime.Ticks, pollCycleTimeStamp, process.StartTime.ToUniversalTime(), threadLevel);
                threadInfoTableCount++;

                if (threadLevel)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        try
                        {
                            addInfoToTable(process, table, thread.Id, thread.TotalProcessorTime.Ticks, pollCycleTimeStamp, thread.StartTime.ToUniversalTime(), threadLevel);
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

        public static List<int> GetTimingEntries(int pollInterval)
        {
            List<int> entries = new List<int>();
            for (int nextEntry = 0; nextEntry < 60; nextEntry += pollInterval)
            {
                entries.Add(nextEntry);
            }

            return entries;
        }

        protected DateTime CalculatePollCycleTimeStamp()
        {
            var now = DateTime.UtcNow;
            var pollCycleTimeStamp = GetRoundedPollCycleTimeStamp(now, pollInterval);

            // We need to make sure that between the current poll cycle time stamp and the
            // previous poll cycle time stamp there is at least one poll interval elapsed.
            if (previousPollCycleTimeStamp != default(DateTime))
            {
                // So this is not the first poll. Let's do the check and align, if necessary.
                var nextAcceptableTimeStamp = previousPollCycleTimeStamp.AddSeconds(pollInterval);
                if (nextAcceptableTimeStamp > pollCycleTimeStamp)
                {
                    // Align the poll cycle timestamp
                    pollCycleTimeStamp = nextAcceptableTimeStamp;
                }

                if (pollCycleTimeStamp > now.AddSeconds(1))
                {
                    TimeSpan difference = pollCycleTimeStamp - now;
                    Log.Warn("Aligned poll cycle time stamp is more than 1 second later than the current time stamp! Difference: {0}",
                        difference);
                    // Try our best to match up with the desired poll timings.
                    Thread.Sleep(difference);
                }
            }
            previousPollCycleTimeStamp = pollCycleTimeStamp;

            return pollCycleTimeStamp;
        }

        protected static DateTime GetRoundedPollCycleTimeStamp(DateTime now, int pollInterval)
        {
            List<int> timeEntries = GetTimingEntries(pollInterval);
            timeEntries.Reverse();
            // Make sure that the result will be really rounded.
            DateTime prevEntry = now.AddMilliseconds(-now.Millisecond);

            foreach (var entry in timeEntries)
            {
                if (now.Second >= entry)
                {
                    prevEntry = prevEntry.AddSeconds(entry - now.Second);
                    var nextEntry = prevEntry.AddSeconds(pollInterval);
                    if (nextEntry - now < now - prevEntry)
                    {
                        // Next entry is closer in time.
                        return nextEntry;
                    }
                    // Round to the previous entry.
                    return prevEntry;
                }
            }

            // We should never get here.
            Log.Error("Failed to get a rounded poll cycle timestamp! Returning previous now as a default.");
            return now;
        }
    }
}
