using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using PalMon.Output;
using System.IO;
using NLog;

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
            string[] testFiles = { "serverlog-2016-01-28-15-06-00.csv", "serverlog-2016-01-28-15-06-30.csv",
                                   "threadinfo-2016-01-28-15-06-00.csv" };
            Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);

            // Act
            var resultFiles = DBWriter.GetFilesOfSameTable();

            // Assert
            Isolate.Verify.WasCalledWithExactArguments(() => Directory.GetFiles("csv/", "serverlog-*.csv"));
        }

        [TestMethod, Isolated]
        public void TestGetFilesOfSameTable_NoFiles()
        {
            // Arrange
            Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(new string[0]);

            // Act
            var resultFiles = DBWriter.GetFilesOfSameTable();

            // Assert
            Assert.IsTrue(resultFiles.Count == 0);
        }

        [TestMethod, Isolated]
        public void TestGetFilesOfSameTable_NoFilesMatchingPattern()
        {
            // Arrange
            string[] testFiles = { "serverlog1.csv", "serverlog2.csv",
                                   "threadinfo.csv" };
            Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);

            // Act
            var resultFiles = DBWriter.GetFilesOfSameTable();

            // Assert
            // The result is expected to be an empty list, since none of the filenames contain "-" (hyphens).
            Assert.AreEqual(0, resultFiles.Count);
        }
    }


    [TestClass]
    public class DbWriterTests_GetFileOrTableName
    {
        private string testFile = "";

        public DbWriterTests_GetFileOrTableName()
        {
        }

        #region Additional test attributes

        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            testFile = "serverlog-2016-01-28-15-06-00.csv";
        }

        #endregion

        [TestMethod, Isolated]
        public void TestGetFileName()
        {
            // Arrange
            var fullFileName = "csv/" + testFile;

            // Act
            var fileName = DBWriter.GetFileName(fullFileName);

            // Assert
            Assert.AreEqual(testFile, fileName);
        }

        [TestMethod, Isolated]
        public void TestGetFileName_NotInFolder()
        {
            // Arrange
            var fullFileName = testFile;

            // Act
            var fileName = DBWriter.GetFileName(fullFileName);

            // Assert
            Assert.AreEqual(testFile, fileName);
        }

        [TestMethod, Isolated]
        public void TestGetFileName_InTheDeep()
        {
            // Arrange
            var fullFileName = "one/two/three/" + testFile;

            // Act
            var fileName = DBWriter.GetFileName(fullFileName);

            // Assert
            Assert.AreEqual(testFile, fileName);
        }


        [TestMethod, Isolated]
        public void TestGetTableName()
        {
            // Act
            var tableName = DBWriter.GetTableName(testFile);

            // Assert
            Assert.AreEqual("serverlog", tableName);
        }

        [TestMethod, Isolated]
        public void TestGetTableName_InvalidFile()
        {
            // Arrange
            var invalidFile = "threadinfo.csv"; // invalid because it does not contain "-" (hyphen)

            // Act
            var tableName = DBWriter.GetTableName(invalidFile);

            // Assert
            Assert.AreEqual("", tableName);
        }
    }


    [TestClass]
    public class DbWriterTests_MoveToProcessed
    {
        IList<string> testFileList;
        Logger fakeLog;

        public DbWriterTests_MoveToProcessed()
        {
        }

        #region Additional test attributes

        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            testFileList = new List<string>();
            testFileList.Add("csv/serverlog-2016-01-28-15-06-00.csv");
            testFileList.Add("csv/serverlog-2016-01-28-15-06-30.csv");
            testFileList.Add("csv/threadinfo-2016-01-28-15-06-00.csv");

            fakeLog = Isolate.Fake.AllInstances<Logger>();
            Isolate.WhenCalled(() => LogManager.GetCurrentClassLogger()).WillReturn(fakeLog);
        }

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        public void MyTestCleanup()
        {
            testFileList.Clear();
        }

        #endregion

        [TestMethod, Isolated]
        public void TestMoveToProcessed()
        {
            // Arrange
            Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);

            // Act
            DBWriter.MoveToProcessed(testFileList);

            // Assert
            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Exists("anyfile")));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[0], "csv/processed/serverlog-2016-01-28-15-06-00.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[1], "csv/processed/serverlog-2016-01-28-15-06-30.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[2], "csv/processed/threadinfo-2016-01-28-15-06-00.csv"));
        }

        [TestMethod, Isolated]
        public void TestMoveToProcessed_DestinationFileAlreadyExists()
        {
            // Arrange
            Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(true);
            Isolate.WhenCalled(() => File.Delete("anyfile")).IgnoreCall();

            // Act
            DBWriter.MoveToProcessed(testFileList);

            // Assert
            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Delete("anyfile")));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("csv/processed/serverlog-2016-01-28-15-06-00.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("csv/processed/serverlog-2016-01-28-15-06-30.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("csv/processed/threadinfo-2016-01-28-15-06-00.csv"));

            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Exists("anyfile")));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[0], "csv/processed/serverlog-2016-01-28-15-06-00.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[1], "csv/processed/serverlog-2016-01-28-15-06-30.csv"));
            Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[2], "csv/processed/threadinfo-2016-01-28-15-06-00.csv"));
        }

        [TestMethod, Isolated]
        public void TestMoveToProcessed_ExceptionAtFileExists()
        {
            // Arrange
            var testException = new Exception();
            Isolate.WhenCalled(() => File.Exists("anyfile")).WillThrow(testException);

            // Act
            DBWriter.MoveToProcessed(testFileList);

            // Assert
            Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
        }

        [TestMethod, Isolated]
        public void TestMoveToProcessed_ExceptionAtFileDelete()
        {
            // Arrange
            var testException = new Exception();
            Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);
            Isolate.WhenCalled(() => File.Delete("anyfile")).WillThrow(testException);

            // Act
            DBWriter.MoveToProcessed(testFileList);

            // Assert
            Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
        }

        [TestMethod, Isolated]
        public void TestMoveToProcessed_ExceptionAtFileMove()
        {
            // Arrange
            var testException = new Exception();
            Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);
            Isolate.WhenCalled(() => File.Delete("anyfile")).IgnoreCall();
            Isolate.WhenCalled(() => File.Move("source", "destination")).WillThrow(testException);

            // Act
            DBWriter.MoveToProcessed(testFileList);

            // Assert
            Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
            Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
        }
    }
}
