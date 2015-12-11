using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataTableWriter.Connection;

namespace DataTableWriter.UnitTests.Connection
{
    [TestClass]
    public class DbConnectionInfoTests
    {
        [TestMethod]
        public void TestValidPortIsNegative()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = -96,
                Username = "user",
                Password = "secret",
                DatabaseName = "db"
            };
            Assert.IsFalse(connection.Valid());
        }

        [TestMethod]
        public void TestValidPortIsTooLarge()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = 750000,
                Username = "user",
                Password = "secret",
                DatabaseName = "db"
            };
            Assert.IsFalse(connection.Valid());
        }

        [TestMethod]
        public void TestValidParametersAreGood()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = 5000,
                Username = "user",
                Password = "secret",
                DatabaseName = "db"
            };
            Assert.IsTrue(connection.Valid());
        }

        [TestMethod]
        public void TestValidDatabaseNameIsWrong()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = 5000,
                Username = "user",
                Password = "secret",
                DatabaseName = null
            };
            Assert.IsFalse(connection.Valid());
        }

        [TestMethod]
        public void TestValidPasswordIsWrong()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = 5000,
                Username = "user",
                Password = null,
                DatabaseName = "db"
            };
            Assert.IsFalse(connection.Valid());
        }

        [TestMethod]
        public void TestValidUserIsWrong()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = "Server",
                Port = 5000,
                Username = null,
                Password = "secret",
                DatabaseName = "db"
            };
            Assert.IsFalse(connection.Valid());
        }

        [TestMethod]
        public void TestValidServerIsWrong()
        {
            IDbConnectionInfo connection = new DbConnectionInfo
            {
                Server = null,
                Port = 5000,
                Username = "user",
                Password = "secret",
                DatabaseName = "db"
            };
            Assert.IsFalse(connection.Valid());
        }
    }
}