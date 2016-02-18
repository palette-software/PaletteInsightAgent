using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Sampler;

namespace PaletteInsightAgent.Counters
{
    /// <summary>
    /// Represents a generic performance counter interface.
    /// </summary>
    public interface ICounter
    {
        string Category { get; }
        string Counter { get; }
        string Instance { get; }
        string HostName { get; }

        ICounterSample Sample();

        string ToString();
    }

}