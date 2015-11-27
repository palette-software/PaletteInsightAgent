using DataTableWriter.Writers;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TabMon.Counters;
using TabMon.Counters.MBean;
using JavaLong = java.lang.Long;

namespace TabMon.JMXThreadInfoPoller
{
    struct ThreadInfo
    {
        public string host;
        public string instance;
        public long threadId;
        public long cpuTime;
        public long userTime;
        public long allocatedBytes;
        public DateTime pollTimeStamp;
    }

    class JMXThreadInfoAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void poll(ICollection<ICounter> counters, IDataTableWriter writer, object WriteLock)
        {
            long serverLogsTableCount = 0;
            var serverLogsTable = JMXThreadTables.makeJMXThreadInfoTable();
            HashSet<IMBeanClient> polledClients = new HashSet<IMBeanClient>();
            foreach (var counter in counters)
            {
                if (isJMXCounter(counter))
                {
                    var mbeanCounter = (AbstractMBeanCounter)counter;
                    // We poll every mbeanclient once as multiple counters may share the same client 
                    if (!polledClients.Contains(mbeanCounter.MBeanClient))
                    {
                        pollThreadsOfCounter(counter,  serverLogsTable, ref serverLogsTableCount);
                        polledClients.Add(mbeanCounter.MBeanClient);
                    }
                }
            }
            lock (WriteLock)
            {
                if (serverLogsTableCount > 0) writer.Write(serverLogsTable);
            }
        }

        protected bool isJMXCounter(ICounter counter)
        {
            return counter is TabMon.Counters.MBean.AbstractMBeanCounter;
        }

        protected void pollThreadsOfCounter(ICounter genericCounter, DataTable table, ref long serverLogsTableCount)
        {
            AbstractMBeanCounter counter = (AbstractMBeanCounter)genericCounter;
            Log.Debug(String.Format(@"Getting thread info for counter: {0}", counter));

            try
            {
                long[] threadIds = (long[])counter.GetMBeanAttributeValue("AllThreadIds", "java.lang", "type=Threading");
                foreach (long threadId in threadIds)
                {
                    ThreadInfo threadInfo = new ThreadInfo();
                    threadInfo.threadId = threadId;
                    JavaLong javaThreadId = new JavaLong(threadId);
                    threadInfo.cpuTime = ((JavaLong)(counter.InvokeMethod("getThreadCpuTime", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading"))).longValue();
                    threadInfo.userTime = ((JavaLong)(counter.InvokeMethod("getThreadUserTime", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading"))).longValue();
                    threadInfo.allocatedBytes = ((JavaLong)(counter.InvokeMethod("getThreadAllocatedBytes", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading"))).longValue();
                    threadInfo.pollTimeStamp = DateTime.Now;
                    threadInfo.host = counter.Host.ToString();
                    threadInfo.instance = counter.Instance;
                    JMXThreadTables.addToTable(table, threadInfo);
                    serverLogsTableCount++;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format(@"Failed to poll thread info for counter {0}. Exception message: {1}", genericCounter, ex.Message));
            }
        }
    }
}
