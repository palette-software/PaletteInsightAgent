﻿using NLog;
using System;
using System.Diagnostics;
using System.Reflection;
using PalMon.Helpers;
using PalMon.Sampler;

namespace PalMon.Counters.Perfmon
{
    /// <summary>
    /// Represents a Perfmon counter on a machine, possible remote.  This is a thin wrapper over the existing System.Diagnostics PerformanceCounter class.
    /// </summary>
    public sealed class PerfmonCounter : ICounter, IDisposable
    {
        private const string PerfmonCounterType = "Perfmon";
        private const string PerfmonSource = "Perfmon";
        private readonly PerformanceCounter perfmonCounter;
        private bool disposed;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Host Host { get; private set; }
        public string CounterType { get; private set; }
        public string Source { get; private set; }
        public string Category { get; private set; }
        public string Counter { get; private set; }
        public string Instance { get; private set; }
        public string Unit { get; private set; }

        public PerfmonCounter(Host host, string counterCategory, string counterName, string instance, string unit)
        {
            Host = host;
            CounterType = PerfmonCounterType;
            Source = PerfmonSource;
            Category = counterCategory;
            Counter = counterName;
            Instance = instance;
            Unit = unit;
            perfmonCounter = new PerformanceCounter(Category, Counter, Instance, Host.Name);
        }

        ~PerfmonCounter()
        {
            Dispose(false);
        }

        #region Public Methods

        /// <summary>
        /// Sample this Perfmon counter.
        /// </summary>
        /// <returns>A CounterSample containing the value of this counter at the moment of sampling.</returns>
        public ICounterSample Sample()
        {
            try
            {
                var value = perfmonCounter.NextValue();
                return new Sampler.CounterSample(this, value);
            }
            catch (Exception ex)
            {
                Log.Info("Error sampling counter {0}: {1}", this, ex.Message);
                return null;
            }
        }

        public override string ToString()
        {
            return String.Format(@"{0}\{1}\{2}\{3}\{4}", Host, Source, Category, Counter, Instance);
        }

        #endregion Public Methods

        #region IDisposable Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                perfmonCounter.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable Methods
    }
}