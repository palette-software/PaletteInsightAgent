using NLog;
using System.Collections.Generic;
using System.IO;
using PalMon.Counters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PalMon.CounterConfig
{
    /// <summary>
    /// Validates & parses the Counters.yml file into a collection of ICounter objects.
    /// </summary>
    internal static class CounterConfigLoader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static ICollection<ICounter> Load(string pathToConfig)
        {
            // load the defaults from the application
            // since PalMonAgent always sets the current directory to its location,
            // we should always be in the correct folder for this to work
            using (var reader = File.OpenText(pathToConfig))
            {
                var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                var counterConfig = deserializer.Deserialize<List<Counters.Config>>(reader);
                return Counters.Config.ToICounterList(counterConfig);
            }
        }
    }
}