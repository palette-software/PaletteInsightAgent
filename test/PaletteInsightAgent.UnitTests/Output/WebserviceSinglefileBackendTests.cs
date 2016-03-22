using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.Output.OutputDrivers;

namespace PaletteInsightAgentTests.Output
{
    [TestClass]
    public class WebserviceSinglefileBackendTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var expected = "http_requests-2016-03-22--07-42-20.csv";
            var actual = SinglefileBackend.GetFileNameWithoutPart("http_requests-2016-03-22--07-42-20--part0000.csv");
            Assert.AreEqual(expected, actual);
        }
    }
}
