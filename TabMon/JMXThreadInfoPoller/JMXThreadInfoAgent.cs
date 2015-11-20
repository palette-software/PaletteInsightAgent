using DataTableWriter.Writers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TabMon.Counters;
using TabMon.Counters.MBean;
using JavaLong = java.lang.Long;

namespace TabMon.JMXThreadInfoPoller
{
    class JMXThreadInfoAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void poll(ICollection<ICounter> counters, IDataTableWriter writer, object WriteLock)
        {
            HashSet<IMBeanClient> polledClients = new HashSet<IMBeanClient>();
            foreach (var counter in counters)
            {
                if (isJMXCounter(counter))
                {
                    var mbeanCounter = (AbstractMBeanCounter)counter;
                    // We poll every mbeanclient once as multiple counters may share the same client 
                    if (!polledClients.Contains(mbeanCounter.MBeanClient))
                    {
                        pollThreadsOfCounter(counter,  writer, WriteLock);
                        polledClients.Add(mbeanCounter.MBeanClient);
                    }
                }
            }
        }

        protected bool isJMXCounter(ICounter counter)
        {
            return counter is TabMon.Counters.MBean.AbstractMBeanCounter;
        }

        protected void pollThreadsOfCounter(ICounter genericCounter, IDataTableWriter writer, object WriteLock)
        {
            AbstractMBeanCounter counter = (AbstractMBeanCounter)genericCounter;
            Log.Info(String.Format(@"Should get thread info for counter: {0}", counter));
            long[] threadIds = (long[])counter.GetAttributeValue("AllThreadIds", "java.lang", "type=Threading");
            foreach (long threadId in threadIds)
            {
                JavaLong javaThreadId = new JavaLong(threadId);
                var cpuTime = counter.InvokeMethod("getThreadCpuTime", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading");
                var userTime = counter.InvokeMethod("getThreadUserTime", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading");
                var allocatedBytes = counter.InvokeMethod("getThreadAllocatedBytes", new object[] { javaThreadId }, new string[] { "long" }, "java.lang", "type=Threading");
                Log.Info(String.Format(@"Found thread ({0}) with CPU time: {1}, User time: {2}, Allocated bytes: {3}", threadId, cpuTime, userTime, allocatedBytes));
            }
        }
    }
}
