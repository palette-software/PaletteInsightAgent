using javax.management.openmbean;
using TabMon.Helpers;

namespace TabMon.Counters.MBean
{
    /// <summary>
    /// Represents a Java language MBean counter.
    /// </summary>
    internal class JavaHealthCounter : AbstractMBeanCounter
    {
        private const string JavaHealthJmxDomain = "java.lang";
        private const string JavaHealthCounterType = "JVM Health";

        public JavaHealthCounter(IMBeanClient mbeanClient, Host host, string sourceName, string path, string categoryName, string counterName, string instanceName, string unit)
            : base(mbeanClient: mbeanClient, counterType: JavaHealthCounterType, jmxDomain: JavaHealthJmxDomain, host: host, source: sourceName, filter: path, category: categoryName, counter: counterName, instance: instanceName, unit: unit) { }

        public override object GetAttributeValue(string attribute, string domain = null, string path = null)
        {
            object result;

            // The Java Health counters may be nested as CompositeData objects, so we need to be prepared to handle this.
            if (attribute.Contains(@"\"))
            {
                var pathSegments = attribute.Split('\\');
                var parent = pathSegments[0];
                var child = pathSegments[1];
                var compositeData = GetMBeanAttributeValue(parent, domain, path) as CompositeData;
                result = compositeData.get(child);
            }
            else
            {
                result = GetMBeanAttributeValue(attribute, domain, path);
            }

            return result;
        }

    }
}