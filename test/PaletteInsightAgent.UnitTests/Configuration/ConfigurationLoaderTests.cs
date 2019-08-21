using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PaletteInsightAgent.Configuration;


namespace PaletteInsightAgentTests.Configuration
{
    /// <summary>
    /// Configuration Loader Tests
    /// </summary>
    [TestClass]
    public class ConfigurationLoaderTests
    {
        public ConfigurationLoaderTests()
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
        public void TestExtractTableauInstallationFolder_slash()
        {
            var expected = "E:/Program Files/Tableau/Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder("E:/Program Files/Tableau/Tableau Server/worker.1/bin/tabsvc.exe");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_backslash()
        {
            var expected = @"C:\Program Files\Tableau\Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"C:\Program Files\Tableau\Tableau Server\worker.1\bin\tabsvc.exe");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_doublebackslash()
        {
            var expected = @"E:\\Program Files\\Tableau\\Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"E:\\Program Files\\Tableau\\Tableau Server\\worker.1\\bin\\tabsvc.exe");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_longformat_slash()
        {
            var expected = @"D:/Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"""D:/Tableau Server/worker.1/bin/tabsvc.exe"" /SN:tabsvc");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_longformat_backslash()
        {
            var expected = @"D:\Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"""D:\Tableau Server\worker.1\bin\tabsvc.exe"" /SN:tabsvc");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_longformat_doublebackslash()
        {
            var expected = @"D:\\Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"""D:\\Tableau Server\\10.0\\bin\\tabsvc.exe"" /SN:tabsvc");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_primary()
        {
            var expected = @"D:/Program Files/Tableau/Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"D:/Program Files/Tableau/Tableau Server/9.2/bin/tabsvc.exe");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_null()
        {
            Assert.IsNull(Loader.ExtractTableauInstallationFolder(null));
            Assert.IsNull(Loader.ExtractTableauInstallationFolder(""));
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_20182()
        {
            var expected = @"C:\ProgramData\Tableau\Tableau Server";
            var actual = Loader.ExtractTableauInstallationFolder(@"""C:\ProgramData\Tableau\Tableau Server\data\tabsvc\services\tabsvc_0.20182.18.0627.2230\tabsvc\tabsvc.exe"" run");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractTableauInstallationFolder_nonmatching()
        {
            Assert.IsNull(Loader.ExtractTableauInstallationFolder(@"E:\Program Files\Tableau\Tableau Server\worker.1\bin"));
            Assert.IsNull(Loader.ExtractTableauInstallationFolder(@"E:\Program Files\Tableau\Tableau Server\worker.1\bin\tabsvc.bat"));
        }

        [TestMethod]
        public void TestExtractTableauBinFolder_longformat_doublebackslash()
        {
            var expected = @"D:\\Tableau Server\\10.0\\bin\\";
            var actual = Loader.ExtractTableauBinFolder(@"""D:\\Tableau Server\\10.0\\bin\\tabsvc.exe"" /SN:tabsvc");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIsEncrypted()
        {
            Assert.IsTrue(Loader.IsEncrypted("ENC(jdkv8d8fsdkfjs8dfu)"));
        }

        [TestMethod]
        public void TestIsEncrypted_not()
        {
            Assert.IsFalse(Loader.IsEncrypted("onlyread"));
        }

        [TestMethod]
        public void TestLoadProcessData()
        {
            List<ProcessData> processList = Loader.LoadProcessData();
            Assert.IsNotNull(processList);
            Assert.AreEqual(6, processList.Count);
            Assert.AreEqual("Thread", processList[0].Granularity);
            Assert.AreEqual("vizqlserver", processList[0].Name);
            Assert.AreEqual("Thread", processList[1].Granularity);
            Assert.AreEqual("dataserver", processList[1].Name);
        }

        [TestMethod]
        public void TestLoadDefaultLogFolders()
        {
            List<LogFolder> folderList = Loader.LoadDefaultLogFolders();
            Assert.AreEqual(7, folderList.Count);
            Assert.AreEqual(@"tabsvc\vizqlserver\Logs", folderList[0].Directory);
            Assert.AreEqual("*.txt", folderList[0].Filter);
        }

        [TestMethod]
        public void TestLoadConfigFile()
        {
            PaletteInsightConfiguration config = Loader.LoadConfigFile("config/Config.yml");
            Assert.AreEqual(true, config.UseRepoPolling);
            Assert.AreEqual("http://localhost:9000", config.Webservice.Endpoint);
        }

        // This test would pass if we used Encoding.UTF8 instead of Encoding.ASCII.
        [TestMethod]
        public void TestUnicodeBase64Convert()
        {
            string utfUserName = "Sébastien";
            var additionalEntropy = Encoding.ASCII.GetBytes("c4a1c275-42a3-4cc5-91e0-b55cad0be835");
            var protectedName = Convert.ToBase64String(ProtectedData.Protect(Encoding.ASCII.GetBytes(utfUserName), additionalEntropy, DataProtectionScope.LocalMachine));
            var unprotectedName = Encoding.ASCII.GetString(ProtectedData.Unprotect(Convert.FromBase64String(protectedName), additionalEntropy, DataProtectionScope.LocalMachine));
            Assert.AreEqual(utfUserName, unprotectedName);
        }
    }
}
