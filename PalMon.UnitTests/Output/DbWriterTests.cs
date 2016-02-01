using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using PalMon.Output;
using System.IO;

namespace PalMonTests.Output
{
    /// <summary>
    /// DB writer tests
    /// </summary>
    [TestClass]
    public class DbWriterTests
    {
        public DbWriterTests()
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
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod, Isolated]
        public void TestGetFilesOfSameTable()
        {
            // Arrange
            string file1 = "serverlog-2016-01-28-15-06-00.csv";
            string file2 = "serverlog-2016-01-28-15-06-30.csv";
            string file3 = "threadinfo-2016-01-28-15-06-00.csv";
            string[] testFiles = { file1, file2, file3 };
            string[] actualFiles = { file1, file2 };

            Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);

            // Act
            var resultFiles = DBWriter.GetFilesOfSameTable();

            // Assert
            Isolate.Verify.WasCalledWithExactArguments(() => Directory.GetFiles("csv/", "serverlog-*.csv"));
        }
    }
}
