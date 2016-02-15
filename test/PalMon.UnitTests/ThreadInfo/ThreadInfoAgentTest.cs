using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PalMon.ThreadInfoPoller;
using TypeMock.ArrangeActAssert;
using System.Diagnostics;
using TypeMock.ArrangeActAssert.Fluent;
using NLog;
using TypeMock;

namespace PalMonTests.ThreadInfo
{
    /// <summary>
    /// Summary description for ThreadInfoAgentTest
    /// </summary>
    [TestClass]
    public class ThreadInfoAgentTest
    {
        public ThreadInfoAgentTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod, Isolated]
        public void ShouldInsertValueForProcess()
        {
            var process = Isolate.Fake.AllInstances<Process>();
            Isolate.WhenCalled(() => process.TotalProcessorTime).WillReturn(new TimeSpan(15600));
            Isolate.WhenCalled(() => process.StartTime).WillReturn(System.DateTime.Now);
            var table = ThreadTables.makeThreadInfoTable();
            IBox<long> count = Args.Ref<long>(0);

            var agent = new ThreadInfoAgent();
            Isolate.Invoke.Method(agent, "pollThreadCountersOfProcess", new Process(), false, table, count);
        }

        [TestMethod, Isolated]
        public void ShouldNotThrowWhenProcessExits()
        {
            var process = Isolate.Fake.AllInstances<Process>();
            var FakeLogger = Isolate.Fake.Instance<Logger>();
            ObjectState state = new ObjectState(typeof(ThreadInfoAgent));
            state.SetField("Log", FakeLogger);
            Isolate.WhenCalled(() => process.TotalProcessorTime).WillThrow(new InvalidOperationException());
            var table = ThreadTables.makeThreadInfoTable();
            IBox<long> count = Args.Ref<long>(0);

            var agent = new ThreadInfoAgent();
            Isolate.Invoke.Method(agent, "pollThreadCountersOfProcess", new Process(), false, table, count);
            Isolate.Verify.WasNotCalled(() => FakeLogger.Error("Failed to poll thread info for process {0}! Exception message: {1}", "", ""));
        }
    }
}
