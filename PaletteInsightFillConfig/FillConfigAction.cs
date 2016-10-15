using System;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using YamlDotNet.Serialization;

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
                    bool anyChangeOnUI = false;

                    logWriter.Write("Acquiring Insight Server URL...\n");
                    string insightServerUrl = session.CustomActionData["InsightServerUrl"];
                    logWriter.Write(String.Format("Insight Sercer URL: {0}\n", insightServerUrl));
                    logWriter.Write("Acquiring install folder...\n");

                    string installFolder = session.CustomActionData["InstallDir"];
                    var configFilePath = Path.Combine(installFolder, "Config", "Config.yml");
                    logWriter.Write(String.Format("Config file path: {0}\n", configFilePath));

                    string insightLicenseKey = session.CustomActionData["InsightLicenseKey"];
                    logWriter.Write(String.Format("License key: {0}\n", insightLicenseKey));

                    var agentConfig = Loader.LoadConfigFile(configFilePath);
                    if (agentConfig == null)
                    {
                        logWriter.Write(String.Format("Failed to load config file at path: {0}\n", configFilePath));
                        return ActionResult.Failure;
                    }

                    if (insightLicenseKey != "")
                    {
                        agentConfig.LicenseKey = insightLicenseKey;
                        anyChangeOnUI = true;
                    }

                    if (insightServerUrl != "" && insightServerUrl != "https://")
                    {
                        agentConfig.Webservice.Endpoint = insightServerUrl;
                        anyChangeOnUI = true;
                    }


                    if (anyChangeOnUI)
                    {
                        // Only modify the Config.yml, if there is any change. This is very important,
                        // so that silent installs never overwrite the config file.
                        using (TextWriter configWriter = File.CreateText(configFilePath))
                        {
                            var serializer = new Serializer();
                            serializer.Serialize(configWriter, agentConfig);
                        }
                    }

                    logWriter.Write("We are gooood!\n");
                }
                catch (Exception e)
                {
                    logWriter.Write(String.Format("Failed to apply UI based settings to config file! Exception: {0}\n", e));
                    return ActionResult.Failure;
                }
            }
            

            return ActionResult.Success;
        }
    }
}
