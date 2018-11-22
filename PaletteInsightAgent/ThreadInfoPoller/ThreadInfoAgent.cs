using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using PaletteInsightAgent.Output;
using PaletteInsightAgent.Configuration;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text.RegularExpressions;
using PaletteInsightAgent.Helpers;


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
        public ulong readOperationCount;
        public ulong writeOperationCount;
        public ulong otherOperationCount;
        public ulong readTransferCount;
        public ulong writeTransferCount;
        public ulong otherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    };

    class ThreadInfoAgent
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS lpIoCounters);

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
                OutputSerializer.Write(threadInfoTable, false);
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
                                        DateTime startTimeStamp, bool threadLevel, IO_COUNTERS ioCounters)
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
            threadInfo.process = StringUtil.ReplaceSubString(process.ProcessName, "^(control-|run-)");

            threadInfo.readOperationCount = ioCounters.ReadOperationCount;
            threadInfo.writeOperationCount = ioCounters.WriteOperationCount;
            threadInfo.otherOperationCount = ioCounters.OtherOperationCount;
            threadInfo.readTransferCount = ioCounters.ReadTransferCount;
            threadInfo.writeTransferCount = ioCounters.WriteTransferCount;
            threadInfo.otherTransferCount = ioCounters.OtherTransferCount;

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
                IO_COUNTERS ioCounters;
                if (GetProcessIoCounters(process.Handle, out ioCounters) == false)
                {
                    int error_code = Marshal.GetLastWin32Error();
                    if (error_code != 0)
                    {
                        // In theory this should only happen when the process already exited
                        // and we leave the IO counters 0 for this measurement here as it would throw an exception later
                        // when getting the TotalProcessorTime for the process so nothing to do here.
                    }
                }

                // Store the total processor time of the whole process so that we can do sanity checks on the sum of thread cpu times
                addInfoToTable(process, table, -1, process.TotalProcessorTime.Ticks, pollCycleTimeStamp, process.StartTime.ToUniversalTime(), threadLevel, ioCounters);
                threadInfoTableCount++;

                if (threadLevel)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        try
                        {
                            addInfoToTable(process, table, thread.Id, thread.TotalProcessorTime.Ticks, pollCycleTimeStamp, thread.StartTime.ToUniversalTime(), threadLevel, new IO_COUNTERS());
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

        protected DateTime CalculatePollCycleTimeStamp()
        {
            var now = DateTime.UtcNow;
            var pollCycleTimeStamp = TruncateToPreviousBeat(now, pollInterval);

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
                    Log.Debug("Aligned poll cycle time stamp is more than 1 second later than the current time stamp! Difference: {0}",
                        difference);
                    // Try our best to match up with the desired poll timings.
                    Thread.Sleep(difference);
                }
            }
            previousPollCycleTimeStamp = pollCycleTimeStamp;

            return pollCycleTimeStamp;
        }

        protected static DateTime TruncateToPreviousBeat(DateTime now, int pollInterval)
        {
            int dueTimeToNextBeat = PaletteInsightAgent.CalculateDueTime(pollInterval);
            if (dueTimeToNextBeat == 0)
            {
                // Just on the beat.
                return now;
            }

            // Return the previous (already elapsed) beat.
            return now.AddMilliseconds(dueTimeToNextBeat - pollInterval * 1000);
        }
    }
}
