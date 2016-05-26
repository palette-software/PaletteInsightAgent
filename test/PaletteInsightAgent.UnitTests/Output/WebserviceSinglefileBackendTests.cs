using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.Output.OutputDrivers;

namespace PaletteInsightAgentTests.Output
{
    [TestClass]
    public class WebserviceSinglefileBackendTests
    {
        [TestMethod]
        public void TestGetFileNameWithoutPart_csv()
        {
            var expected = "http_requests-2016-03-22--07-42-20.csv";
            var actual = SinglefileBackend.GetFileNameWithoutPart("http_requests-2016-03-22--07-42-20--part0000.csv");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetFileNameWithoutPart_csvgz()
        {
            var expected = "http_requests-2016-03-22--07-42-20.csv.gz";
            var actual = SinglefileBackend.GetFileNameWithoutPart("http_requests-2016-03-22--07-42-20--part0000.csv.gz");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetFileNameWithoutPart_withSequence()
        {
            var expected = "countersamples-2016-05-23--13-15-15.csv.gz";
            var actual = SinglefileBackend.GetFileNameWithoutPart("countersamples-2016-05-23--13-15-15--seq0000--part0000.csv.gz");
            Assert.AreEqual(expected, actual);
        }
    }
}
