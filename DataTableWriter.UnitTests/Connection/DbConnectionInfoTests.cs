using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataTableWriter.Connection;

namespace DataTableWriter.UnitTests.Connection
{
    [TestClass]
    public class DbConnectionInfoTests
    {
        [TestMethod]
        public void ValidPortIsNegative()
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
        public void ValidPortIsTooLarge()
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
        public void ValidParametersAreGood()
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
        public void ValidDatabaseNameIsWrong()
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
        public void ValidPasswordIsWrong()
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
        public void ValidUserIsWrong()
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
        public void ValidServerIsWrong()
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