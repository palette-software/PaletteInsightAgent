﻿using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using PalMon.Counters;
using PalMon.Helpers;

namespace PalMon.CounterConfig
{
    /// <summary>
    /// Validates & parses the Counters.config file into a collection of ICounter objects.
    /// </summary>
    internal static class CounterConfigLoader
    {
        private const string PathToSchema = @"Resources\CountersConfig.xsd";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Loads the Counters.config file, validates it against the XSD schema, and news up the appropriate CounterConfigReader object for each root counter type node.
        /// </summary>
        /// <param name="pathToConfig">The path to the Counters.config file.</param>
        /// <param name="hosts">The target hosts to load counters for.</param>
        /// <returns>Collection of all valid counters in Counters.config across the set of given hosts.</returns>
        public static ICollection<ICounter> Load(string pathToConfig, IEnumerable<Host> hosts)
        {
            var counters = new Collection<ICounter>();

            // Load the document & validate against internal schema.
            var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema
                };

            var doc = new XmlDocument();
            try
            {
                settings.Schemas.Add("", PathToSchema);
                var reader = XmlReader.Create(pathToConfig, settings);
                doc.Load(reader);
            }
            catch (FileNotFoundException ex)
            {
                throw new ConfigurationErrorsException(String.Format("Could not find file '{0}'.", ex.Message));
            }
            catch (XmlException ex)
            {
                throw new ConfigurationErrorsException(String.Format("Malformed XML in' {0}': {1}", pathToConfig, ex.Message));
            }
            catch (XmlSchemaValidationException ex)
            {
                throw new ConfigurationErrorsException(String.Format("Failed to validate '{0}': {1} (Line {2})", pathToConfig, ex.Message, ex.LineNumber));
            }
            Log.Debug("Successfully validated '{0}' against '{1}'.", pathToConfig, PathToSchema);

            // Set the root element & begin loading counters.
            var documentRoot = doc.DocumentElement.SelectSingleNode("/Counters");
            var counterRootNodes = documentRoot.SelectNodes("child::*");
            foreach (XmlNode counterRootNode in counterRootNodes)
            {
                var counterType = counterRootNode.Name;
                Log.Debug("Loading {0} counters..", counterType);
                var configReader = CounterConfigReaderFactory.CreateConfigReader(counterType);

                foreach (var host in hosts)
                {
                    var countersInNode = configReader.LoadCounters(counterRootNode, host);
                    Log.Info("Loaded {0} {1} {2} on {3}.", countersInNode.Count, counterType, "counter".Pluralize(countersInNode.Count), host.Name);
                    counters.AddRange(countersInNode);
                }
            }

            return counters;
        }
    }
}