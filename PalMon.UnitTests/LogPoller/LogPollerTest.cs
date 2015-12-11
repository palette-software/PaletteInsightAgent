using System;
using System.IO;
using NSubstitute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PalMon.LogPoller;
//using Remotion.Mixins;


//[assembly: Mix(typeof(Directory), typeof(DirectoryTest), MixinKind = MixinKind.Used)]

//public static class DirectoryTest
//{
//    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
//    {
//        string[] testFiles = { "testFile1", "testFile2" };
//        return testFiles;
//    }
//}

namespace UnitTestLogPoller
{
    [TestClass]
    public class LogPollerTest
    {
        [TestMethod]
        public void TestLogFileWatcher()
        {
            //var fileStreamSub = Substitute.For<FileStream>();
            //var directorySub  = Substitute.For<DirectoryTest>();


            LogFileWatcher watcher = new LogFileWatcher("testFolder", "testFilter");
            Assert.IsTrue(watcher != null);
        }
    }
}
