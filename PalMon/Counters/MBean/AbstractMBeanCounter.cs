using javax.management;
using NLog;
using System;
using System.Reflection;
using PalMon.Helpers;
using PalMon.Sampler;

namespace PalMon.Counters.MBean
{
    /// <summary>
    /// Base class that other MBeanCounter classes should inherit from.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AbstractMBeanCounter : ICounter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Host Host { get; private set; }
        public string CounterType { get; private set; }
        public string Source { get; private set; }
        public string Category { get; private set; }
        public string Counter { get; private set; }
        public string Instance { get; private set; }
        public string Unit { get; private set; }
        public IMBeanClient MBeanClient { get; protected set; }
        protected string JmxDomain { get; set; }
        protected string Path { get; set; }

        protected AbstractMBeanCounter(IMBeanClient mbeanClient, string counterType, string jmxDomain, Host host, string source, string filter,
                                       string category, string counter, string instance, string unit)
        {
            Host = host;
            CounterType = counterType;
            Source = source;
            Category = category;
            Counter = counter;
            Instance = instance;
            Unit = unit;
            MBeanClient = mbeanClient;
            JmxDomain = jmxDomain;
            Path = filter;
        }

        #region Public Methods

        /// <summary>
        /// Sample this counter.
        /// </summary>
        /// <returns>ICounterSample containing the sampled attribute value of this counter.</returns>
        public ICounterSample Sample()
        {
            String counterStr = "";
            try
            {
                counterStr = GetAttributeValue(Counter).ToString();
                var value = float.Parse(counterStr);
                return new CounterSample(this, value);
            }
            catch (FormatException)
            {
                Log.Warn("Failed to parse counter {0} from value {1}", this, counterStr);
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn("Error sampling counter {0}. Exception message: {1}", this, ex.Message);
                return null;
            }
        }

        public override string ToString()
        {
            return String.Format(@"{0}\{1}\{2}:{3}\{4}\{5}", Host, Source, JmxDomain, Path, Counter, Instance);
        }

        /// <summary>
        /// Retrieve the value of the attribute with the given name for this counter.
        /// </summary>
        /// <param name="attribute">Name of the attribute to sample.</param>
        /// <returns>A generic object containing the sampled value for the given attribute.</returns>
        public abstract object GetAttributeValue(string attribute, string domain = null, string path = null);

        public object GetMBeanAttributeValue(string attribute, string domain, string path)
        {
            var obj = getMBeanObjectName(domain, path);
            return MBeanClient.GetAttributeValue(obj, attribute);
        }

        public object InvokeMethod(string methodname, object[] args = null, string[] signature = null, string domain = null, string path = null)
        {
            var obj = getMBeanObjectName(domain, path);
            return MBeanClient.InvokeMethod(obj, methodname, args, signature);
        }
        #endregion Public Methods

        #region Protected Methods

        protected ObjectName getMBeanObjectName(string domain = null, string path = null)
        {
            // Let's allow to use mbean objects outside the scope of the counter
            if (domain == null)
            {
                domain = JmxDomain;
            }

            if (path == null)
            {
                path = Path;
            }

            // Find MBean object.
            var objectNames = MBeanClient.QueryObjects(domain, path);

            // Validated MBean object was found.
            if (objectNames.Count < 1)
            {
                throw new ArgumentException("Unable to query MBean.");
            }

            return objectNames[0];
        }

        #endregion Protected Methods
    }
}