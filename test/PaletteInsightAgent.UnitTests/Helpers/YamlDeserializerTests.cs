using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Configuration;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PaletteInsightAgentTests.Helpers
{
    [TestClass]
    public class YamlDeserializerTests
    {
        [TestMethod]
        public void TestProcessesYamlDeserialize()
        {
            using (var reader = File.OpenText(Loader.PROCESSES_DEFAULT_FILE))
            {
                IDeserializer deserializer = YamlDeserializer.Create(new UnderscoredNamingConvention());
                List<ProcessData> processList = deserializer.Deserialize<List<ProcessData>>(reader);
                Assert.IsNotNull(processList);
                Assert.AreEqual(6, processList.Count);
                Assert.AreEqual("Thread", processList[0].Granularity);
                Assert.AreEqual("vizqlserver", processList[0].Name);
                Assert.AreEqual("Thread", processList[1].Granularity);
                Assert.AreEqual("dataserver", processList[1].Name);
            }
        }
    }
}
