using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.Helpers;

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
            var actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "fiel\\015\\012d";
            source = "fiel\r\nd";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "\\\\roppantjoteszt";
            source = "\\roppantjoteszt";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "\\\\vertical\\013";
            source = "\\vertical\v";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);
        }

    }
}
