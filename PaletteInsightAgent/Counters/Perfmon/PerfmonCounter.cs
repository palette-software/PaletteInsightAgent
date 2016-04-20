using NLog;
using System;
using System.Diagnostics;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Sampler;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.Net;

namespace PaletteInsightAgent.Counters
{
    /// <summary>
    /// Represents a Perfmon counter on a machine, possible remote.  This is a thin wrapper over the existing System.Diagnostics PerformanceCounter class.
    /// </summary>
    public sealed class PerfmonCounter : ICounter, IDisposable
    {
        private readonly PerformanceCounter perfmonCounter;
        private bool disposed;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string currentHostName = Dns.GetHostName();

        public string Category { get; private set; }
        public string Counter { get; private set; }
        public string Instance { get; private set; }
        public string HostName { get { return currentHostName; } }

        public PerfmonCounter(string counterCategory, string counterName, string instance)
        {
            Category = counterCategory;
            Counter = counterName;
            Instance = instance;
            perfmonCounter = new PerformanceCounter(Category, Counter, Instance, HostName);
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
            return String.Format(@"{0}\{1}\{2}\{3}", HostName, Category, Counter, Instance);
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

    // Yaml types
    public class Config
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [YamlMember(Alias = "Categories")]
        public ICollection<Category> Categories { get; set; }
        public static ICollection<ICounter> ToICounterList(ICollection<Config> configs)
        {
            List<ICounter> ret = new List<ICounter>();
            foreach (var config in configs)
            {
                foreach (var category in config.Categories)
                {
                    foreach (var counter in category.Counters)
                    {
                        if (counter.Instances != null && counter.Instances.Count > 0)
                        {
                            foreach (var instance in counter.Instances)
                            {
                                try
                                {
                                    ret.Add(new PerfmonCounter(category.Name, counter.Name, instance));
                                }
                                catch (Exception ex)
                                {
                                    Log.Warn("Failed to load performance counter! Category: {0} Counter: {1} Instance: {2}. Error message: {3}",
                                     category.Name, counter.Name, instance, ex.Message);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                ret.Add(new PerfmonCounter(category.Name, counter.Name, null));
                            }
                            catch (Exception ex)
                            {
                                Log.Warn("Failed to load performance counter! Category: {0} Counter: {1}. Error message: {2}",
                                 category.Name, counter.Name, ex.Message);
                            }
                        }
                    }

                }
            }
            return ret;
        }
    }

    public class Category
    {
        [YamlMember(Alias = "Name")]
        public string Name { get; set; }

        [YamlMember(Alias = "Counters")]
        public ICollection<Counter> Counters { get; set; }
    }

    public class Counter
    {
        [YamlMember(Alias = "Name")]
        public string Name { get; set; }

        [YamlMember(Alias = "Instances")]
        public ICollection<string> Instances { get; set; }
    }
}