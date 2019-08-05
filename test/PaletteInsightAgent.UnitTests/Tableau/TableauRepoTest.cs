using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.RepoTablesPoller;
using PaletteInsightAgent.Output;

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

    [TestClass]
    public class TableauRepoTest_GetMaxQuery
    {
        private DbConnectionInfo dbConnInfo;
        private Tableau9RepoConn repoConn;

        private string tableName;
        private string field;
        private string prevMax;
        private string filter;

        public TableauRepoTest_GetMaxQuery()
        {
            dbConnInfo = new DbConnectionInfo
                {
                    Server = "localhost",
                    Port = 8060,
                    Username = "readonly",
                    Password = "password",
                    DatabaseName = "workgroup"
                };
            repoConn = new Tableau9RepoConn(dbConnInfo, 100000);

            field = "event_id";
            tableName = "events";
        }

        [TestMethod]
        public void GetMaxQuery()
        {
            string prevMax = "555";
            string filter = "progress = 100";

            string expected = @"
                select max(event_id)
                from
                    (
                    select event_id
                    from events
                        where 1 = 1
                        and event_id > '555'
                        and progress = 100
                        order by event_id asc
                        limit 100000
                    ) as iq
                ;";
            Assert.AreEqual(expected, this.repoConn.GetMaxQuery(this.tableName, this.field, filter, prevMax));
        }

        [TestMethod]
        public void GetMaxQuery_no_prevMax()
        {
            string prevMax = null;
            string filter = "progress = 100";

            string expected = @"
                select max(event_id)
                from
                    (
                    select event_id
                    from events
                        where 1 = 1
                        
                        and progress = 100
                        order by event_id asc
                        
                    ) as iq
                ;";
            Assert.AreEqual(expected, this.repoConn.GetMaxQuery(this.tableName, this.field, filter, prevMax));
        }

        [TestMethod]
        public void GetMaxQuery_no_filter()
        {
            string prevMax = "555";
            string filter = null;

            string expected = @"
                select max(event_id)
                from
                    (
                    select event_id
                    from events
                        where 1 = 1
                        and event_id > '555'
                        
                        order by event_id asc
                        limit 100000
                    ) as iq
                ;";
            Assert.AreEqual(expected, this.repoConn.GetMaxQuery(this.tableName, this.field, filter, prevMax));
        }

        [TestMethod]
        public void GetMaxQuery_no_filter_no_prevMax()
        {
            string prevMax = null;
            string filter = null;

            string expected = @"
                select max(event_id)
                from
                    (
                    select event_id
                    from events
                        where 1 = 1
                        
                        
                        order by event_id asc
                        
                    ) as iq
                ;";
            Assert.AreEqual(expected, this.repoConn.GetMaxQuery(this.tableName, this.field, filter, prevMax));
        }
    }
}
