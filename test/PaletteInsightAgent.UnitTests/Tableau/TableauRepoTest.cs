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
            Assert.AreEqual(stringified, "123");
        }
    }
}
