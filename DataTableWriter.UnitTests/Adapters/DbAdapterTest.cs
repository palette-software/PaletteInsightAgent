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
    /// <summary>
    /// Summary description for DbAdapterTest
    /// </summary>
    [TestClass]
    public class DbAdapterTest
    {

        //[TestMethod]
        //public void TestConstructorShouldCallOpen()
        //{
        //    var driverSub = Substitute.For<IDbDriver>();
        //    var connectionInfoStub = Substitute.For<IDbConnectionInfo>();
        //    var connectionStub = Substitute.For<IDbConnection>();

        //    driverSub.BuildConnection(connectionInfoStub).Returns(connectionStub);
        //    var adapter = new DbAdapter(driverSub, connectionInfoStub);
        //    connectionStub.Received().Open();
        //}

        [TestMethod]
        public void TestIsConnectionOpenConnectionIsClosed()
        {

        }
    }
}
