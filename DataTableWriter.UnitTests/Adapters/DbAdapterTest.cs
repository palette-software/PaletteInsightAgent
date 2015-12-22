using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using DataTableWriter;
using DataTableWriter.Drivers;
using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using System.Data;

namespace DataTableWriter.UnitTests.Adapters
{
    [TestClass]
    public class DbAdapterTest
    {
        [TestClass]
        public class MethodAddColumn
        {
            [TestMethod]
            public void ShouldAddColumn()
            {
                Assert.Fail();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenErrorHappened()
            {
                Assert.Fail();
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
