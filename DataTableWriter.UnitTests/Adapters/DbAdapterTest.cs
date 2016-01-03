using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using DataTableWriter;
using DataTableWriter.Drivers;
using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using System.Data;
using System.Data.Common;
using TypeMock.ArrangeActAssert;

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
            private DataTable GetSchema1()
            {
                var schema = new DataTable("TABLE1");
                schema.Columns.Add(new DataColumn("COL1")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL1",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL2")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL2",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL3")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL3",
                    AllowDBNull = false
                });

                return schema;
            }

            private DataTable GetSchema2()
            {
                var schema = new DataTable("TABLE2");
                schema.Columns.Add(new DataColumn("COL1")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL1",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL2")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL2",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL3")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL3",
                    AllowDBNull = false
                });

                return schema;
            }

            private DataTable GetSchema3()
            {
                var schema = new DataTable("TABLE3");
                schema.Columns.Add(new DataColumn("COL1")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL1",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL2")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL2",
                    AllowDBNull = false
                });

                schema.Columns.Add(new DataColumn("COL4")
                {
                    DataType = typeof(string),
                    DefaultValue = "VAL4",
                    AllowDBNull = true
                });

                return schema;
            }


            [TestMethod]
            public void ShouldAddNonExistingColumns()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(GetSchema2());
                Isolate.WhenCalled(() => adapter.AddColumn(null, null)).IgnoreCall();
                adapter.AddColumnsToTableToMatchSchema("TABLE", GetSchema1());
                Isolate.Verify.WasCalledWithArguments(() => adapter.AddColumn(null, null)).Matching(
                    args => (args[0] as string).Equals("TABLE1") &&
                            (args[1] as DataColumn).ColumnName.Equals("COL3") &&
                            (args[1] as DataColumn).DataType == typeof(string) &&
                            (args[1] as DataColumn).DefaultValue.Equals("VAL3") &&
                            (args[1] as DataColumn).AllowDBNull == false
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionWhenTryingToAddNonNullableColumn()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(GetSchema3());
                Isolate.WhenCalled(() => adapter.AddColumn(null, null)).IgnoreCall();
                adapter.AddColumnsToTableToMatchSchema("TABLE", GetSchema1());
               
            }

            [TestMethod]
            public void ShouldNotAddColumnsIfSchemaAlreadyContainsThem()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(GetSchema1());
                Isolate.WhenCalled(() => adapter.AddColumn(null, null)).IgnoreCall();
                adapter.AddColumnsToTableToMatchSchema("TABLE", GetSchema2());
                Isolate.Verify.WasNotCalled(() => adapter.AddColumn(null, null));
            }
        }

        [TestClass]
        public class MethodCloseConnection
        {

            [TestMethod]
            public void ShouldCallConnectionClose()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.CloseConnection();

                connection.Received().Close();

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
                const string query = "EXIST_TABLE_QUERY";
                const string table = "TABLE";
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                command.ExecuteScalar().Returns(null);
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQuerySelectTable(Arg.Any<string>()).Returns(query);

                var adapter = new DbAdapter(driver, connectionInfo);
                var isExist = adapter.ExistsTable(table);
                Assert.AreEqual(connection, command.Connection);
                Assert.AreEqual(query, command.CommandText);
                command.Received().ExecuteScalar();
                driver.Received().BuildQuerySelectTable(table);
                Assert.IsFalse(isExist);
            }

            [TestMethod]
            public void ShouldReturnTrueWhenTableDoesExist()
            {
                const string query = "EXIST_TABLE_QUERY";
                const string table = "TABLE";
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                command.ExecuteScalar().Returns(new DataColumn());
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQuerySelectTable(Arg.Any<string>()).Returns(query);

                var adapter = new DbAdapter(driver, connectionInfo);
                var isExist = adapter.ExistsTable(table);
                Assert.AreEqual(connection, command.Connection);
                Assert.AreEqual(query, command.CommandText);
                command.Received().ExecuteScalar();
                driver.Received().BuildQuerySelectTable(table);
                Assert.IsTrue(isExist);
            }

            [TestMethod]
            [ExpectedException(typeof(DbException), AllowDerivedTypes = true)]
            public void ShouldThrowExceptionWhenCouldntExecuteNonQuery()
            {
                const string query = "EXIST_TABLE_QUERY";
                const string table = "TABLE";
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                command
                    .When(c => c.ExecuteScalar())
                    .Do(c => { throw new FakeDbException(); });
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQuerySelectTable(Arg.Any<string>()).Returns(query);

                var adapter = new DbAdapter(driver, connectionInfo);
                var isExist = adapter.ExistsTable(table);
            }
        }

        [TestClass]
        public class MethodGetSchema
        {

            [TestMethod]
            public void ShouldReturnTheTable()
            {
                var tableName = "TABLE";
                var query = "GET_SCHEMA_QUERY";
                var table = new DataTable();
                table.Columns.Add("name", typeof(string));
                table.Columns.Add("type", typeof(string));
                table.Columns.Add("nullable", typeof(string));
                table.Rows.Add(new object[] {"col1", "text", "false"});
                table.Rows.Add(new object[] {"col2", "text", "false"});
                table.Rows.Add(new object[] {"col3", "boolean", "false"});

                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);
                driver.BuildQueryColumnNamesAndTypes(tableName).Returns(query);
                driver.MapToSystemType("text").Returns(typeof(System.String));
                driver.MapToSystemType("boolean").Returns(typeof(System.Boolean));
                command.ExecuteReader().Returns(table.CreateDataReader());

                var adapter = new DbAdapter(driver, connectionInfo);
                var result = adapter.GetSchema(tableName);
                Assert.AreEqual(table.Rows.Count, result.Columns.Count);
                foreach (DataRow row in table.Rows)
                {
                    Assert.IsTrue(result.Columns.Contains(row["name"] as string));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(DbException), AllowDerivedTypes = true)]
            public void ShouldThrowExceptionWhenCouldntGetTable()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                var command = Substitute.For<IDbCommand>();
                command
                    .When(c => c.ExecuteReader())
                    .Do(c => { throw new FakeDbException(); });
                connection.CreateCommand().Returns(command);
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.GetSchema("TABLE");
            }
        }

        [TestClass]
        public class MethodExistsColumn
        {
            [TestMethod]
            public void ShouldReturnTrueWhenColumnExists()
            {
                var tableName = "TABLE";
                var table = new DataTable(tableName);
                table.Columns.Add("col1", typeof(string));
                table.Columns.Add("col2", typeof(string));
                table.Columns.Add("col3", typeof(string));
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(table);
                var col = new DataColumn("col1");
                Assert.IsTrue(adapter.ExistsColumn(tableName, col));
            }

            [TestMethod]
            public void ShouldReturnFalseWhenColumnDoesNotExist()
            {
                var tableName = "TABLE";
                var table = new DataTable(tableName);
                table.Columns.Add("col1", typeof(string));
                table.Columns.Add("col2", typeof(string));
                table.Columns.Add("col3", typeof(string));
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(table);
                var col = new DataColumn("col1");
                Assert.IsFalse(adapter.ExistsColumn(tableName, col));
            }
        }


        [TestClass]
        public class MethodIsConnectionOpen
        {

            [TestMethod]
            public void ShouldReturnFalseWhenConnectionIsNotOpen()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                connection.State.Returns(ConnectionState.Closed);
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Assert.IsFalse(adapter.IsConnectionOpen());
            }

            [TestMethod]
            public void ShouldReturnTrueWhenConnectionOpen()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                connection.State.Returns(ConnectionState.Open);
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                Assert.IsTrue(adapter.IsConnectionOpen());
            }
        }

        [TestClass]
        public class MethodOpenConnection
        {

            [TestMethod]
            public void ShouldCallConnectionOpen()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                connection.State.Returns(ConnectionState.Open);
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.OpenConnection();
                connection.Received().Open();
            }

            [TestMethod]
            [ExpectedException(typeof(DbConnectionException))]
            public void ShouldThrowExceptionIfCouldntOpenConnection()
            {
                var driver = Substitute.For<IDbDriver>();
                var connectionInfo = Substitute.For<IDbConnectionInfo>();
                var connection = Substitute.For<IDbConnection>();
                connection.State.Returns(ConnectionState.Closed);
                driver.BuildConnection(connectionInfo).Returns(connection);

                var adapter = new DbAdapter(driver, connectionInfo);
                adapter.OpenConnection();
                connection.Received().Open();
            }
        }
    }
}
