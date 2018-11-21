using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaletteInsightAgent.Helpers;


namespace PaletteInsightAgentTests.Helpers
{   [TestClass]
    class StringUtilTest
    {    
            [TestMethod]
            public void PrefixStringStartsWithControl()
            {
                var name1 = "control-tabadmincontroller";
                var name2 = "control-filestore";
                var result = StringUtil.ReplaceSubString(name1, "^(control-|run-)");
                var result2 = StringUtil.ReplaceSubString(name2, "^(control-|run-)");
                Assert.AreEqual("tabadmincontroller", result);
                Assert.AreEqual("filestore", result2);
            }

            [TestMethod]
            public void PrefixStringStartsWithRun()
            {
                var name1 = "run-controller";
                var name2 = "run-file-store";
                var result = StringUtil.ReplaceSubString(name1, "^(control-|run-)");
                var result2 = StringUtil.ReplaceSubString(name2, "^(control-|run-)");
                Assert.AreEqual("controller", result);
                Assert.AreEqual("file-store", result2);
            }

            [TestMethod]
            public void PrefixStringStartsWithNative()
            {
                var name1 = "nativeapi_controller";
                var name2 = "nativeapi_filestore";
                var result = StringUtil.ReplaceSubString(name1, "^nativeapi_");
                var result2 = StringUtil.ReplaceSubString(name2, "^nativeapi_");
                Assert.AreEqual("controller", result);
                Assert.AreEqual("filestore", result2);
            }

            [TestMethod]
            public void PrefixStringNativeInMiddle()
            {
                var name1 = "nativeapi_contronativeapi_ller";
                var name2 = "nativeapi_servercontrolnativeapi_admin";
                var result = StringUtil.ReplaceSubString(name1, "^nativeapi_");
                var result2 = StringUtil.ReplaceSubString(name2, "^nativeapi_");
                Assert.AreEqual("contronativeapi_ller", result);
                Assert.AreEqual("servercontrolnativeapi_admin", result2);
            }

            [TestMethod]
            public void PrefixStringControlInMiddle()
            {
                var name1 = "control-admincontrol-poller";
                var name2 = "control-servercontrol-nativeapi_admin";
                var result = StringUtil.ReplaceSubString(name1, "^(control-|run-)");
                var result2 = StringUtil.ReplaceSubString(name2, "^(control-|run-)");
                Assert.AreEqual("admincontrol-poller", result);
                Assert.AreEqual("servercontrol-nativeapi_admin", result2);
            }

    }
   
}
