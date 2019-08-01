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
            expected = "fiel\\r\\nd";
            source = "fiel\r\nd";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "\\\\roppantjoteszt";
            source = "\\roppantjoteszt";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);

            // replace new lines
            expected = "\\\\vertical\v";
            source = "\\vertical\v";
            actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EscapeForCsv_Comma()
        {
            var expected = "multi\\,\\, comma\\, in field";
            var source = "multi,, comma, in field";
            var actual = GreenplumCsvEscaper.EscapeField(source);
            Assert.AreEqual(expected, actual);
        }

    }
}
