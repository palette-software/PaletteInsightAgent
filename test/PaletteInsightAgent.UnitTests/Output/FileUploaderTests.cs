using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.Output;
using System.IO;
using NLog;

namespace PaletteInsightAgentTests.Output
{
    /// <summary>
    /// DB writer tests
    /// </summary>
    [TestClass]
    public class FileUploaderTests
    {
        public FileUploaderTests()
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

        [TestMethod]
        public void TestGetFileNameWithoutPart_csv()
        {
            var expected = "http_requests-2016-03-22--07-42-20.csv";
            var actual = FileUploader.GetFileNameWithoutPart("http_requests-2016-03-22--07-42-20--part0000.csv");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetFileNameWithoutPart_csvgz()
        {
            var expected = "http_requests-2016-03-22--07-42-20.csv.gz";
            var actual = FileUploader.GetFileNameWithoutPart("http_requests-2016-03-22--07-42-20--part0000.csv.gz");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetFileNameWithoutPart_withSequence()
        {
            var expected = "countersamples-2016-05-23--13-15-15.csv.gz";
            var actual = FileUploader.GetFileNameWithoutPart("countersamples-2016-05-23--13-15-15--seq0000--part0000.csv.gz");
            Assert.AreEqual(expected, actual);
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

        //[TestMethod, Isolated]
        //public void TestGetFilesOfSameTable()
        //{
        //    // Arrange
        //    string[] testFiles = { "serverlog-2016-01-28-15-06-00.csv", "serverlog-2016-01-28-15-06-30.csv",
        //                           "threadinfo-2016-01-28-15-06-00.csv" };
        //    Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);

        //    // Act
        //    var resultFiles = FileUploader.GetFilesOfSameTable();

        //    // Assert
        //    Isolate.Verify.WasCalledWithExactArguments(() => Directory.GetFiles("data/", "serverlog-*.csv"));
        //}

        //[TestMethod, Isolated]
        //public void TestGetFilesOfSameTable_FilterFilesThatAreBeingWritten()
        //{
        //    // Arrange
        //    string[] testFiles = { "serverlog-2016-01-28-15-06-00.csv", "serverlog-2016-01-28-15-06-30.csv",
        //                           "threadinfo-2016-01-28-15-06-00.csv", "serverlog-2016-01-28-15-06-30.csv.writing" };
        //    string[] serverLogs = { testFiles[0], testFiles[1], testFiles[3] };
        //    Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);
        //    Isolate.WhenCalled(() => Directory.GetFiles("data/", "serverlog-*.csv")).WithExactArguments().WillReturn(serverLogs);

        //    // Act
        //    var resultFiles = FileUploader.GetFilesOfSameTable();

        //    // Assert
        //    Isolate.Verify.WasCalledWithExactArguments(() => Directory.GetFiles("data/", "serverlog-*.csv"));
        //    List<string> expectedFiles = new List<string>();
        //    expectedFiles.Add(testFiles[0]);
        //    expectedFiles.Add(testFiles[1]);
        //    Assert.AreEqual(2, resultFiles.Count);
        //    for (int i = 0; i < resultFiles.Count; i++)
        //    {
        //        Assert.AreEqual(expectedFiles[i], resultFiles[i]);
        //    }
        //}

        //[TestMethod, Isolated]
        //public void TestGetFilesOfSameTable_NoFiles()
        //{
        //    // Arrange
        //    Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(new string[0]);

        //    // Act
        //    var resultFiles = FileUploader.GetFilesOfSameTable();

        //    // Assert
        //    Assert.IsTrue(resultFiles.Count == 0);
        //}

        //[TestMethod, Isolated]
        //public void TestGetFilesOfSameTable_NoFilesMatchingPattern()
        //{
        //    // Arrange
        //    string[] testFiles = { "serverlog1.csv", "serverlog2.csv",
        //                           "threadinfo.csv" };
        //    Isolate.WhenCalled(() => Directory.GetFiles("anyfolder", "anyfilter")).WillReturn(testFiles);

        //    // Act
        //    var resultFiles = FileUploader.GetFilesOfSameTable();

        //    // Assert
        //    // The result is expected to be an empty list, since none of the filenames contain "-" (hyphens).
        //    Assert.AreEqual(0, resultFiles.Count);
        //}
    }


    [TestClass]
    public class DbWriterTests_GetFileOrTableName
    {
        //private string testFile = "";

        public DbWriterTests_GetFileOrTableName()
        {
        }

        #region Additional test attributes

        //// Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //    testFile = "serverlog-2016-01-28-15-06-00.csv";
        //}

        #endregion

        //[TestMethod, Isolated]
        //public void TestGetFileName()
        //{
        //    // Arrange
        //    var fullFileName = "data/" + testFile;

        //    // Act
        //    var fileName = FileUploader.GetFileName(fullFileName);

        //    // Assert
        //    Assert.AreEqual(testFile, fileName);
        //}

        //[TestMethod, Isolated]
        //public void TestGetFileName_NotInFolder()
        //{
        //    // Arrange
        //    var fullFileName = testFile;

        //    // Act
        //    var fileName = FileUploader.GetFileName(fullFileName);

        //    // Assert
        //    Assert.AreEqual(testFile, fileName);
        //}

        //[TestMethod, Isolated]
        //public void TestGetFileName_InTheDeep()
        //{
        //    // Arrange
        //    var fullFileName = "one/two/three/" + testFile;

        //    // Act
        //    var fileName = FileUploader.GetFileName(fullFileName);

        //    // Assert
        //    Assert.AreEqual(testFile, fileName);
        //}


        //[TestMethod, Isolated]
        //public void TestGetTableName()
        //{
        //    // Act
        //    var tableName = FileUploader.GetTableName(testFile);

        //    // Assert
        //    Assert.AreEqual("serverlog", tableName);
        //}

        //[TestMethod, Isolated]
        //public void TestGetTableName_InvalidFile()
        //{
        //    // Arrange
        //    var invalidFile = "threadinfo.csv"; // invalid because it does not contain "-" (hyphen)

        //    // Act
        //    var tableName = FileUploader.GetTableName(invalidFile);

        //    // Assert
        //    Assert.AreEqual("", tableName);
        //}
    }


    [TestClass]
    public class DbWriterTests_MoveToProcessed
    {
        //IList<string> testFileList;
        //Logger fakeLog;

        public DbWriterTests_MoveToProcessed()
        {
        }

        #region Additional test attributes

        //// Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //    testFileList = new List<string>();
        //    testFileList.Add("data/serverlog-2016-01-28-15-06-00.csv");
        //    testFileList.Add("data/serverlog-2016-01-28-15-06-30.csv");
        //    testFileList.Add("data/threadinfo-2016-01-28-15-06-00.csv");

        //    fakeLog = Isolate.Fake.AllInstances<Logger>();
        //    Isolate.WhenCalled(() => LogManager.GetCurrentClassLogger()).WillReturn(fakeLog);
        //}

        //// Use TestCleanup to run code after each test has run
        //// [TestCleanup()]
        //public void MyTestCleanup()
        //{
        //    testFileList.Clear();
        //}

        #endregion

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed()
    //    {
    //        // Arrange
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Exists("anyfile")));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[0], "data/processed/serverlog-2016-01-28-15-06-00.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[1], "data/processed/serverlog-2016-01-28-15-06-30.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[2], "data/processed/threadinfo-2016-01-28-15-06-00.csv"));
    //    }

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed_DestinationFileAlreadyExists()
    //    {
    //        // Arrange
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(true);
    //        Isolate.WhenCalled(() => File.Delete("anyfile")).IgnoreCall();

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Delete("anyfile")));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("data/processed/serverlog-2016-01-28-15-06-00.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("data/processed/serverlog-2016-01-28-15-06-30.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Delete("data/processed/threadinfo-2016-01-28-15-06-00.csv"));

    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => File.Exists("anyfile")));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[0], "data/processed/serverlog-2016-01-28-15-06-00.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[1], "data/processed/serverlog-2016-01-28-15-06-30.csv"));
    //        Isolate.Verify.WasCalledWithExactArguments(() => File.Move(testFileList[2], "data/processed/threadinfo-2016-01-28-15-06-00.csv"));
    //    }

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed_DestinationFolderDoesNotExist()
    //    {
    //        // Arrange
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);
    //        var destinationFolder = "data/processed/";
    //        Isolate.WhenCalled(() => Directory.Exists(destinationFolder)).WithExactArguments().WillReturn(false);
    //        // Only report in the first round that the destination folder is missing
    //        Isolate.WhenCalled(() => Directory.Exists(destinationFolder)).WithExactArguments().WillReturn(true);

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        // Make sure that the not-existing folder got created
    //        Isolate.Verify.WasCalledWithExactArguments(() => Directory.CreateDirectory(destinationFolder));
    //    }

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed_ExceptionAtFileExists()
    //    {
    //        // Arrange
    //        var testException = new Exception();
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillThrow(testException);

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
    //    }

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed_ExceptionAtFileDelete()
    //    {
    //        // Arrange
    //        var testException = new Exception();
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);
    //        Isolate.WhenCalled(() => File.Delete("anyfile")).WillThrow(testException);

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
    //    }

    //    [TestMethod, Isolated]
    //    public void TestMoveToProcessed_ExceptionAtFileMove()
    //    {
    //        // Arrange
    //        var testException = new Exception();
    //        Isolate.WhenCalled(() => File.Exists("anyfile")).WillReturn(false);
    //        Isolate.WhenCalled(() => File.Delete("anyfile")).IgnoreCall();
    //        Isolate.WhenCalled(() => File.Move("source", "destination")).WillThrow(testException);

    //        // Act
    //        foreach (var file in testFileList)
    //        {
    //            FileUploader.MoveToFolder(file, FileUploader.ProcessedPath);
    //        }

    //        // Assert
    //        Isolate.Verify.WasCalledWithAnyArguments(() => fakeLog.Error(testException, "any line"));
    //        Assert.AreEqual(3, Isolate.Verify.GetTimesCalled(() => fakeLog.Error(testException, "any line")));
    //    }
    }
}
