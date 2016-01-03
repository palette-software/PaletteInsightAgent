using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using DataTableWriter;
using DataTableWriter.Drivers;
using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using System.Data;
using System.Data.Common;

namespace DataTableWriter.UnitTests.Adapters
{
    [TestClass]
    public class DbAdapterTest
    {
        public class FakeDbException : DbException { }

        [TestClass]
        public class MethodAddColumn
        {
            [TestMethod]
            public void ShouldAddColumn()
            {
                const string query = "ADD_COLUMN_QUERY"; 
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQueryAddColumnToTable(Arg.Any<string>(), Arg.Any<DataColumn>()).Returns(query);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.AddColumn("TABLE", new DataColumn());

                Assert.AreEqual(connection, command.Connection);
                Assert.AreEqual(query, command.CommandText);
                command.Received().ExecuteNonQuery();
            }

            [TestMethod]
            [ExpectedException(typeof(DbException), AllowDerivedTypes = true)]
            public void ShouldThrowExceptionWhenErrorHappened()
            {
                const string query = "ADD_COLUMN_QUERY";
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                command
                    .When(c => c.ExecuteNonQuery())
                    .Do(c => { throw new FakeDbException(); });
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQueryAddColumnToTable(Arg.Any<string>(), Arg.Any<DataColumn>()).Returns(query);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.AddColumn("TABLE", new DataColumn());
            }
        }

        [TestClass]
        public class MethodAddColumnsToTableToMatchSchema
        {

            [TestMethod]
            public void ShouldAddNonExistingColumns()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenTryingToAddNonNullableColumn()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldNotAddColumnsIfSchemaAlreadyContainsThem()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodCloseConnection
        {

            [TestMethod]
            public void ShouldCallConnectionClose()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodCreateTable
        {

            [TestMethod]
            public void ShouldCallDriverFunctionsWhenGenerateIdentityFalse()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldCallDriverFunctionsWhenGenerateIdentityTrue()
            {
                Assert.Fail();
            }

            [TestMethod] public void ShouldThrowExceptionWhenCouldntExecuteNonQuery()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodExistsTable
        {

            [TestMethod]
            public void ShouldReturnFalseWhenTableDoesNotExist()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldReturnTrueWhenTableDoesExist()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenCouldntExecuteNonQuery()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodGetSchema
        {

            [TestMethod]
            public void ShouldReturnTheTable()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldReturnTrueWhenTableDoesExist()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenCouldntGetTable()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodIsConnectionOpen
        {

            [TestMethod]
            public void ShouldReturnFalseWhenConnectionIsNotOpen()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldReturnTrueWhenConnectionOpen()
            {
                Assert.Fail();
            }
        }

        [TestClass]
        public class MethodOpenConnection
        {

            [TestMethod]
            public void ShouldCallConnectionOpen()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldThrowExceptionIfCouldntOpenConnection()
            {
                Assert.Fail();
            }
        }
    }
}
