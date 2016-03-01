using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using System.IO;
using PaletteInsightAgent.Output;

namespace PaletteInsightAgentTests.Output
{
    [TestClass]
    public class CsvSerializerTests
    {
        [TestMethod]
        public void EscapeForCsv_ReturnsField()
        {
            // leave field without special characters
            var expected = "field";
            var source = "field";
            var actual = CsvSerializer.EscapeForCsv(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "fiel\\015\\012d";
            source = "fiel\r\nd";
            actual = CsvSerializer.EscapeForCsv(source);
            Assert.AreEqual(expected, actual);
        }

    }
}
