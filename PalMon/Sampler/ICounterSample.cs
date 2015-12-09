using PalMon.Counters;

namespace PalMon.Sampler
{
    /// <summary>
    /// Describes a sampled performance counter.
    /// </summary>
    public interface ICounterSample
    {
        ICounter Counter { get; }
        object SampleValue { get; }
    }
}