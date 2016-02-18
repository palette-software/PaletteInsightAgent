using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.LogPoller;
using TypeMock.ArrangeActAssert;

namespace UnitTestLogPoller
{
    [TestClass]
    public class LogPollerTest
    {
        [TestMethod, Isolated]
        public void TestLogFileWatcher()
        {
            // Arrange
            string[] testFiles = { "testFile1", "testFile2" };
            string testFolder = "testFolder";
            string testFilter = "testFilter";
            Isolate.WhenCalled(() => Directory.GetFiles(testFolder, testFilter)).WillReturn(testFiles);


            // Act
            LogFileWatcher watcher = new LogFileWatcher(testFolder, testFilter);

            // Assert
            Assert.IsNotNull(watcher);
            //LogFileWatcher.stateOfFiles.ContainsKey(testFiles[0]);
        }
    }
}
