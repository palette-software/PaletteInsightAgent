using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.RepoTablesPoller;


namespace PaletteInsightAgentTests.Tableau
{
    [TestClass]
    public class RepoPollAgentTest
    {
        public RepoPollAgentTest()
        {
        }

        [TestMethod]
        public void CompareMaxIds_int_left()
        {
            string idA = "123";
            string idB = "1123";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void CompareMaxIds_int_equal()
        {
            string idA = "123";
            string idB = "123";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CompareMaxIds_int_right()
        {
            string idA = "123";
            string idB = "23";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void CompareMaxIds_date_left()
        {
            string idA = "2019-05-28 11:00:47.499";
            string idB = "2019-15-28 11:00:47.499";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void CompareMaxIds_date_equal()
        {
            string idA = "2019-05-28 11:00:47.499";
            string idB = "2019-05-28 11:00:47.499";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CompareMaxIds_date_right()
        {
            string idA = "2019-05-28 11:01:47.499";
            string idB = "2019-05-28 11:00:47.499";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void CompareMaxIds_null_left()
        {
            string idA = null;
            string idB = "-1123";
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void CompareMaxIds_null_right()
        {
            string idA = "123";
            string idB = null;
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void CompareMaxIds_null_both()
        {
            string idA = null;
            string idB = null;
            var result = RepoPollAgent.CompareMaxIds(idA, idB);
            Assert.IsTrue(result == 0);
        }
    }
}
