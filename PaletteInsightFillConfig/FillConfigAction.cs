﻿using System;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using YamlDotNet.Serialization;

using PaletteInsightAgent.Configuration;


namespace PaletteInsightFillConfig
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult FillConfig(Session session)
        {
            try
            {
                // session.Log logs to the file specified by the '/log' parameter while running 'msiexec'
                session.Log("Filling up Palette Insight Agent config file based on the parameters specified during the installer UI.");
                bool anyChangeOnUI = false;

                string installFolder = session.CustomActionData["InstallDir"];
                var configFilePath = Path.Combine(installFolder, "Config", "Config.yml");
                session.Log("Insight agent's config file path: {0}", configFilePath);

                string insightServerUrl = session.CustomActionData["InsightServerUrl"];
                session.Log("Insight Server URL: '{0}'", insightServerUrl);

                string insightAuthToken = session.CustomActionData["InsightAuthToken"];
                session.Log("Insight Auth Token: '{0}'", insightAuthToken);

                var agentConfig = Loader.LoadConfigFile(configFilePath);
                if (agentConfig == null)
                {
                    session.Log("Failed to load config file at path: {0}", configFilePath);
                    return ActionResult.Failure;
                }

                if (insightAuthToken != "")
                {
                    agentConfig.InsightAuthToken = insightAuthToken;
                    anyChangeOnUI = true;
                    session.Log("Set InsightAuthToken to: {0}", insightAuthToken);
                }

                if (insightServerUrl != "" && insightServerUrl != "https://")
                {
                    agentConfig.Webservice.Endpoint = insightServerUrl;
                    anyChangeOnUI = true;
                    session.Log("Set Webservice.Endpoint to: {0}", insightServerUrl);
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
                    session.Log("Successfully applied changes on Insight Agent's config file.");
                }
                else
                {
                    session.Log("No changes were proposed for the Insight Agent's config file. Leave it untouched.");
                }
            }
            catch (Exception e)
            {
                session.Log("Failed to apply UI based settings to config file! Exception: {0}", e);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}
