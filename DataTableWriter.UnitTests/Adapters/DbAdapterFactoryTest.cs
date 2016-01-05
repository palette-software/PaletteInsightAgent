using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using DataTableWriter.Drivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace DataTableWriter.UnitTests.Adapters
{
    [TestClass]
    public class DbAdapterFactoryTest
    {
        [TestMethod]
        public void GetInstanceShouldReturnAdapterWithPostgresDriver()
        {
            var connInfo = Substitute.For<IDbConnectionInfo>();
            connInfo.Port = 5000;
            connInfo.Server = "local";
            connInfo.Username = "user";
            connInfo.Password = "pass";
            connInfo.DatabaseName = "db";
            var adapter = DbAdapterFactory.GetInstance(DbDriverType.Postgres, connInfo);
            Assert.IsInstanceOfType(adapter.Driver, typeof(PostgresDriver));
            Assert.AreEqual(connInfo, adapter.ConnectionInfo);
        }
        [TestMethod]
        public void GetInstanceSShouldReturnAdapterWithOracleDriver()
        {
            var connInfo = Substitute.For<IDbConnectionInfo>();
            connInfo.Port = 5000;
            connInfo.Server = "local";
            connInfo.Username = "user";
            connInfo.Password = "pass";
            connInfo.DatabaseName = "db";
            var adapter = DbAdapterFactory.GetInstance(DbDriverType.Oracle, connInfo);
            Assert.IsInstanceOfType(adapter.Driver, typeof(OracleDriver));
            Assert.AreEqual(connInfo, adapter.ConnectionInfo);
        }

        [TestMethod]
        public void GetInstanceSShouldReturnAdapterWithMsSqlDriver()
        {
            var connInfo = Substitute.For<IDbConnectionInfo>();
            connInfo.Port = 5000;
            connInfo.Server = "local";
            connInfo.Username = "user";
            connInfo.Password = "pass";
            connInfo.DatabaseName = "db";
            var adapter = DbAdapterFactory.GetInstance(DbDriverType.MsSQL, connInfo);
            Assert.IsInstanceOfType(adapter.Driver, typeof(MsSQLDriver));
            Assert.AreEqual(connInfo, adapter.ConnectionInfo);
        }
    }
}