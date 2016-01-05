using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using DataTableWriter.Drivers;
using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using System.Data;
using System.Data.Common;
using TypeMock.ArrangeActAssert;
using TypeMock;

namespace DataTableWriter.UnitTests.Adapters
{
    [TestClass]
    public class DbAdapterTest
    {
        public class FakeDbException : DbException { }

        [TestMethod]
        public void AddColumnShouldAddColumn()
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
        [Isolated]
        [ExpectedException(typeof(FakeDbException))]
        public void AddColumnShouldThrowExceptionWhenErrorHappened()
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

        private DataTable GetSchema1()
        {
            var schema = new DataTable("TABLE1");
            schema.Columns.Add(new DataColumn("COL1")
            {
                DataType = typeof(string),
                DefaultValue = "VAL1",
                AllowDBNull = true
            });

            schema.Columns.Add(new DataColumn("COL2")
            {
                DataType = typeof(string),
                DefaultValue = "VAL2",
                AllowDBNull = true
            });

            schema.Columns.Add(new DataColumn("COL3")
            {
                DataType = typeof(string),
                DefaultValue = "VAL3",
                AllowDBNull = true
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
                AllowDBNull = true
            });

            schema.Columns.Add(new DataColumn("COL2")
            {
                DataType = typeof(string),
                DefaultValue = "VAL2",
                AllowDBNull = true
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
                AllowDBNull = true
            });

            schema.Columns.Add(new DataColumn("COL2")
            {
                DataType = typeof(string),
                DefaultValue = "VAL2",
                AllowDBNull = true
            });

            schema.Columns.Add(new DataColumn("COL4")
            {
                DataType = typeof(string),
                DefaultValue = "VAL4",
                AllowDBNull = false
            });

            return schema;
        }


        [TestMethod]
        public void AddColumnsToTableToMatchSchemaShouldAddNonExistingColumns()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);

            var adapter = new DbAdapter(driver, connectionInfo);
            Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(GetSchema2());
            adapter.AddColumnsToTableToMatchSchema("TABLE", GetSchema1());
            Isolate.Verify.WasCalledWithArguments(() => adapter.AddColumn(null, null)).Matching(
                args =>
                {
                    var table = args[0] as string;
                    var column = args[1] as DataColumn;
                    return table.Equals("TABLE1") &&
                        column.ColumnName.Equals("COL3") &&
                        column.DataType == typeof(string) &&
                        column.DefaultValue.Equals("VAL3") &&
                        column.AllowDBNull == true;
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddColumnsToTableToMatchSchemaShouldThrowExceptionWhenTryingToAddNonNullableColumn()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);

            var adapter = new DbAdapter(driver, connectionInfo);
            Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(GetSchema1());
            Isolate.WhenCalled(() => adapter.AddColumn(null, null)).IgnoreCall();
            adapter.AddColumnsToTableToMatchSchema("TABLE", GetSchema3());

        }

        [TestMethod]
        public void AddColumnsToTableToMatchSchemaShouldNotAddColumnsIfSchemaAlreadyContainsThem()
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

        [TestMethod]
        public void CloseConnectionShouldCallConnectionClose()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);

            var adapter = new DbAdapter(driver, connectionInfo);
            adapter.CloseConnection();

            connection.Received().Close();

        }

        private DataTable getSchema(string tableName)
        {
            var schema = new DataTable(tableName);
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
        [TestMethod]
        public void CreateTableShouldCallDriverFunctionsWhenGenerateIdentityFalse()
        {
            const string query = "CREATE_TABLE_QUERY";
            const string tableName = "TABLE";
            var schema = getSchema(tableName);
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            var command = Substitute.For<IDbCommand>();
            connection.CreateCommand().Returns(command);
            driver.BuildConnection(connectionInfo).Returns(connection);
            driver.BuildQueryCreateTable(Arg.Any<string>(), Arg.Any<System.Collections.Generic.List<string>>())
                .Returns(query);

            var adapter = new DbAdapter(driver, connectionInfo);
            adapter.CreateTable(schema, false);

            driver.Received().BuildQueryCreateTable(tableName, Arg.Any<System.Collections.Generic.List<string>>());
            command.Received().ExecuteNonQuery();
            Assert.AreEqual(query, command.CommandText);

        }

        [TestMethod]
        public void CreateTableShouldCallDriverFunctionsWhenGenerateIdentityTrue()
        {
            const string query = "CREATE_TABLE_QUERY";
            const string tableName = "TABLE";
            var schema = getSchema(tableName);
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            var command = Substitute.For<IDbCommand>();
            connection.CreateCommand().Returns(command);
            driver.BuildConnection(connectionInfo).Returns(connection);
            driver.BuildQueryCreateTable(Arg.Any<string>(), Arg.Any<System.Collections.Generic.List<string>>())
                .Returns(query);

            var adapter = new DbAdapter(driver, connectionInfo);
            adapter.CreateTable(schema, true);

            driver.Received().BuildQueryCreateTable(tableName, Arg.Any<System.Collections.Generic.List<string>>());
            driver.Received().GetIdentityColumnSpecification();
            command.Received().ExecuteNonQuery();
            Assert.AreEqual(query, command.CommandText);
        }

        [TestMethod]
        [Isolated]
        [ExpectedException(typeof(FakeDbException))]
        public void CreateTableShouldThrowExceptionWhenCouldntExecuteNonQuery()
        {
            const string query = "CREATE_TABLE_QUERY";
            const string tableName = "TABLE";
            var schema = getSchema(tableName);
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            var command = Substitute.For<IDbCommand>();
            command
                .When(c => c.ExecuteNonQuery())
                .Do(c => { throw new FakeDbException(); });
            connection.CreateCommand().Returns(command);
            driver.BuildConnection(connectionInfo).Returns(connection);

            var adapter = new DbAdapter(driver, connectionInfo);
            adapter.CreateTable(schema, true);
        }

        [TestMethod]
        public void ExistsTableShouldReturnFalseWhenTableDoesNotExist()
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
        public void ExistsTableShouldReturnTrueWhenTableDoesExist()
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
        [Isolated]
        [ExpectedException(typeof(FakeDbException))]
        public void ExistsTableShouldThrowExceptionWhenCouldntExecuteNonQuery()
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
            adapter.ExistsTable(table);
        }

        [TestMethod]
        public void GetSchemaShouldReturnTheTable()
        {
            var tableName = "TABLE";
            var query = "GET_SCHEMA_QUERY";
            var table = new DataTable();
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("type", typeof(string));
            table.Columns.Add("nullable", typeof(string));
            table.Rows.Add(new object[] { "col1", "text", "false" });
            table.Rows.Add(new object[] { "col2", "text", "false" });
            table.Rows.Add(new object[] { "col3", "boolean", "false" });

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
        [Isolated]
        [ExpectedException(typeof(FakeDbException))]
        public void GetSchemaShouldThrowExceptionWhenCouldntGetTable()
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

        [TestMethod]
        public void IsConnectionOpenShouldReturnFalseWhenConnectionIsNotOpen()
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
        public void IsConnectionOpenShouldReturnTrueWhenConnectionOpen()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            connection.State.Returns(ConnectionState.Open);
            driver.BuildConnection(connectionInfo).Returns(connection);

            var adapter = new DbAdapter(driver, connectionInfo);
            Assert.IsTrue(adapter.IsConnectionOpen());
        }


        [TestMethod]
        public void OpenConnectionShouldCallConnectionOpen()
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
        public void OpenConnectionShouldThrowExceptionIfCouldntOpenConnection()
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

        private DataColumn getColumn()
        {
            return new DataColumn("COLUMN", typeof(string));
        }

        private DataTable getTable(string name)
        {
            var table = new DataTable(name);
            table.Columns.Add("COL1", typeof(string));
            table.Columns.Add("COL2", typeof(string));
            return table;
        }

        [TestMethod]
        public void ExistsColumnShouldReturnTrueWhenColumnExists()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);

            string tableName = "TABLE";
            var table = getTable(tableName);
            var columnToTest = getColumn();
            table.Columns.Add(columnToTest);
            var adapter = new DbAdapter(driver, connectionInfo);
            Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(table);
            Assert.IsTrue(adapter.ExistsColumn(tableName, columnToTest));
        }

        [TestMethod]
        public void ExistsColumnShouldReturnFalseWhenColumnDoesNotExist()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);

            string tableName = "TABLE";
            var table = getTable(tableName);
            var columnToTest = getColumn();
            var adapter = new DbAdapter(driver, connectionInfo);
            Isolate.WhenCalled(() => adapter.GetSchema(null)).WillReturn(table);
            Assert.IsFalse(adapter.ExistsColumn(tableName, columnToTest));
        }

        private DataRow getRow()
        {
            var table = new DataTable("TABLE");
            table.Columns.Add("COL1", typeof(string));
            table.Columns.Add("COL2", typeof(string));
            table.Columns.Add("COL3", typeof(int));
            var row = table.NewRow();
            row["COL1"] = "VAL1";
            row["COL2"] = "VAL2";
            row["COL3"] = 1313;
            return row;
        }

        [TestMethod]
        public void InsertRowShouldHaveRightCommandParameters()
        {
            var driver = Substitute.For<IDbDriver>();
            var connectionInfo = Substitute.For<IDbConnectionInfo>();
            var connection = Substitute.For<IDbConnection>();
            driver.BuildConnection(connectionInfo).Returns(connection);
            var command = Substitute.For<IDbCommand>();
            connection.CreateCommand().Returns(command);

            string tableName = "TABLE";
            var row = getRow();
            var adapter = new DbAdapter(driver, connectionInfo);
            adapter.InsertRow(tableName, row);

            //var commandParameters = command.Parameters.Select(object => )
            //CollectionAssert.IsSubsetOf(command.Parameters, string[] );

        }

        [TestMethod]
        public void InsertRowShouldHaveRightCommandText()
        {

        }

        [TestMethod]
        public void InsertRowShouldSendQuery()
        {

        }

        [TestMethod]
        public void InsertRowShouldHandleDateTimeOffsetType()
        {

        }
    }
}
