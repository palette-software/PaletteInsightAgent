using System.Collections.Generic;
using System.Xml;
using PalMon.Counters;
using PalMon.Helpers;

namespace PalMon.CounterConfig
{
    /// <summary>
    /// Basic interface for an ICounterConfigReader.  Should be able to load counters for a given host just given a root node in an XML tree.
    /// </summary>
    internal interface ICounterConfigReader
    {
        ICollection<ICounter> LoadCounters(XmlNode root, Host host);
    }
}