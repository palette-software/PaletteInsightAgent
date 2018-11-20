using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.ThreadInfoPoller;
namespace PaletteInsightAgentTests.ThreadInfo
{
    /// <summary>
    /// Summary description for ThreadInfoAgentTest
    /// </summary>
    [TestClass]
    public class ThreadInfoAgentTest 
    {
        public void ThreadInfoTest()
        {
        }

        [TestMethod]
        public void StripProcessNameStartWith()
        {
            var processName1 = "control-tabadmincontroller";
            var processName2 = "run-filestore";
            var result = ThreadInfoAgent.StripTableauProcessName(processName1);
            var result2 = ThreadInfoAgent.StripTableauProcessName(processName2);
            Assert.AreEqual("tabadmincontroller", result);
            Assert.AreEqual("filestore", result2);
        }

        [TestMethod]
        public void StripProcessNamePatternInString()
        {
            var processName1 = "control-tabadmincontrol-controller";
            var processName2 = "run-filrun-estore";
            var result = ThreadInfoAgent.StripTableauProcessName(processName1);
            var result2 = ThreadInfoAgent.StripTableauProcessName(processName2);
            Assert.AreEqual("tabadmincontrol-controller", result);
            Assert.AreEqual("filrun-estore", result2);
        }

        [TestMethod]
        public void StripProcessEmptyString()
        {
            var processName1 = "";
            var result = ThreadInfoAgent.StripTableauProcessName(processName1);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void StripProcessDoubleStart()
        {
            var processName1 = "run-control-admin";
            var result = ThreadInfoAgent.StripTableauProcessName(processName1);
            Assert.AreEqual("control-admin", result);
        }
    }
}
