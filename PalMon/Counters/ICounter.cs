﻿using PalMon.Helpers;
using PalMon.Sampler;

namespace PalMon.Counters
{
    /// <summary>
    /// Represents a generic performance counter interface.
    /// </summary>
    public interface ICounter
    {
        Host Host { get; }
        string CounterType { get; }
        string Source { get; }
        string Category { get; }
        string Counter { get; }
        string Instance { get; }
        string Unit { get; }

        ICounterSample Sample();

        string ToString();
    }
}