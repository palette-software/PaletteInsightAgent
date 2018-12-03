using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.RepoTablesPoller;

namespace PaletteInsightAgentTests.Tableau
{
    [TestClass]
    public class TableauRepoTest
    {
        public TableauRepoTest()
        {
        }

        [TestMethod]
        public void StringifyMax_int()
        {
            int id = 123;
            var stringified = Tableau9RepoConn.StringifyMax(id);
            Assert.AreEqual("123", stringified);
        }

        [TestMethod]
        public void StringifyMax_string()
        {
            string babe = "Shaunie";
            var stringified = Tableau9RepoConn.StringifyMax(babe);
            Assert.AreEqual("Shaunie", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_en_gb()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            DateTime en_gb = DateTime.ParseExact("2009-05-08 14:40:52,31", "yyyy-MM-dd HH:mm:ss,ff",
            System.Globalization.CultureInfo.InvariantCulture);
            var stringified = Tableau9RepoConn.StringifyMax(en_gb);
            Assert.AreEqual("2009-05-08 14:40:52.310", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_hu_hu()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("hu-HU");
            DateTime hu_hu = new DateTime(2016, 08, 17, 13, 22, 56, 123);
            var stringified = Tableau9RepoConn.StringifyMax(hu_hu);
            Assert.AreEqual("2016-08-17 13:22:56.123", stringified);
        }
    }
}
