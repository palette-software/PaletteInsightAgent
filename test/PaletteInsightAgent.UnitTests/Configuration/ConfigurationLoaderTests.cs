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
    }
}
