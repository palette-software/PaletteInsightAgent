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
            DateTime en_gb = new DateTime(2016, 08, 17, 13, 22, 56);
            var stringified = Tableau9RepoConn.StringifyMax(en_gb);
            Assert.AreEqual("2016-08-17 13:22:56Z", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_hu_hu()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("hu-HU");
            DateTime hu_hu = new DateTime(2016, 08, 17, 13, 22, 56);
            var stringified = Tableau9RepoConn.StringifyMax(hu_hu);
            Assert.AreEqual("2016-08-17 13:22:56Z", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_en_us()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            DateTime en_us = new DateTime(2016, 08, 17, 13, 22, 56);
            var stringified = Tableau9RepoConn.StringifyMax(en_us);
            Assert.AreEqual("2016-08-17 13:22:56Z", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_ko()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ko");
            DateTime ko = new DateTime(2016, 08, 17, 13, 22, 56);
            var stringified = Tableau9RepoConn.StringifyMax(ko);
            Assert.AreEqual("2016-08-17 13:22:56Z", stringified);
        }

        [TestMethod]
        public void StringifyMax_date_nl_nl()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nl-NL");
            DateTime nl_nl = new DateTime(2016, 08, 17, 13, 22, 56);
            var stringified = Tableau9RepoConn.StringifyMax(nl_nl);
            Assert.AreEqual("2016-08-17 13:22:56Z", stringified);
        }
    }
}
