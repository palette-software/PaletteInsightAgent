using javax.management.openmbean;
using System;
using TabMon.Helpers;

namespace TabMon.Counters.MBean
{
    /// <summary>
    /// Represents a Tableau MBean counter.  These counters expose their information as a composite data object obtained via a 'getPerformanceMetrics' method.
    /// </summary>
    internal class TableauHealthCounter : AbstractMBeanCounter
    {
        private const string TableauHealthJmxDomain = "tableau.health.jmx";
        private const string TableauHealthCounterType = "Tableau Server Health";

        public TableauHealthCounter(IMBeanClient mbeanClient, Host host, string sourceName, string path, string categoryName, string counterName, string instanceName, string unit)
            : base(mbeanClient: mbeanClient, counterType: TableauHealthCounterType, jmxDomain: TableauHealthJmxDomain, host: host, source: sourceName, filter: path, category: categoryName, counter: counterName, instance: instanceName, unit: unit) { }

        public override object GetAttributeValue(string attribute, string domain = null, string path = null)
        {
            var obj = getMBeanObjectName(domain, path);
            // Grab associated attributes.
            var attributes = MBeanClient.InvokeMethod(obj, "getPerformanceMetrics") as CompositeData;

            // Look up the attribute we care about & do some wonky parsing to convert it from Java CompositeData into C# float.
            return attributes.get(attribute);
        }

    }
}