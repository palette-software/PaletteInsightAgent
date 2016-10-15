using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using PaletteInsight.Configuration;

namespace PaletteInsightFillConfig
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult FillConfig(Session session)
        {
            var logFilePath = "msi.log";
            using (TextWriter logWriter = File.CreateText(logFilePath))
            {
                try
                {
                    session.Log("Begin FillConfig");
                    logWriter.Write("Begin FillConfig\n");
                    logWriter.Write("Acquiring Insight Server URL...\n");
                    string insightServerUrl = session.CustomActionData["InsightServerUrl"];
                    logWriter.Write(String.Format("Insight Sercer URL: {0}\n", insightServerUrl));
                    logWriter.Write("Acquiring install folder...\n");
                    //var installFolder = session["INSTALLFOLDER"];

                    string installFolder = session.CustomActionData["InstallDir"];
                    var configFilePath = Path.Combine(installFolder, "Config", "Config.yml");
                    logWriter.Write(String.Format("Config file path: {0}\n", configFilePath));
                    //var configFilePath = "Config.yml";
                    //var configFilePath = @"c:\Program Files (x86)\Palette Insight Agent\Config\Config.yml";

                    var agentConfig = Loader.LoadConfigFile(configFilePath);
                    PaletteInsightConfiguration config = null;
                    try
                    {
                        // deserialize the config
                        using (var reader = File.OpenText(configFilePath))
                        {
                            var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention());
                            config = deserializer.Deserialize<PaletteInsightConfiguration>(reader);
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(String.Format("Error during cofiguration loading: {0} -- {1}", configFilePath, e), "File load error");
                        logWriter.Write(String.Format("Error during cofiguration loading: {0} -- {1}\n", configFilePath, e));
                        return ActionResult.Failure;
                    }

                    if (agentConfig == null)
                    {
                        //MessageBox.Show(String.Format("Config file does not exist at path: {0}", configFilePath),
                        //    "Where is my file?");
                        logWriter.Write(String.Format("Config file does not exist at path: {0}\n", configFilePath));
                        return ActionResult.Failure;
                    }

                    //var insightServerUrl = session["WIXUI_INSIGHTSERVERURL"];
                    agentConfig.Webservice.Endpoint = insightServerUrl;


                    using (TextWriter configWriter = File.CreateText(configFilePath))
                    {
                        var serializer = new Serializer();
                        serializer.Serialize(configWriter, agentConfig);
                    }

                    logWriter.Write("We are gooood!\n");

                    //MessageBox.Show(String.Format("Insight Server URL: {0}\nSaved config to: {1}",
                    //    insightServerUrl, configFilePath), "Debug Dialog");
                }
                catch (Exception e)
                {
                    //MessageBox.Show(String.Format("Failed to apply UI based settings to config file! Exception: {0}\nURL: {1}\nPath: {2}",
                    //    e, session["WIXUI_INSIGHTSERVERURL"], session["CONFIG_YML_PATH"]), "Screeeeeaaaam");
                    logWriter.Write(String.Format("Unhandled exception! Exception: {0}\n", e));
                    return ActionResult.Failure;
                }
            }
            

            return ActionResult.Success;
        }
    }
}
