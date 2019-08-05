using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using NLog;
using PaletteInsightAgent.Configuration;


namespace PaletteInsightAgent
{
    /// <summary>
    /// This class contains all the Tableau control related functionality.
    /// </summary>
    public class Tableau
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public const string YML_READONLY_ENABLED = "pgsql.readonly.enabled";


        public static string tabadminRun(string arguments)
        {
            string tableauBinFolder = Loader.RetrieveTableauBinFolder();
            string tabadmin = Path.Combine(tableauBinFolder, "tabadmin.exe");
            Log.Info("Path to tabadmin.exe: {0}", tabadmin);
            if (!File.Exists(tabadmin))
            {
                throw new Exception("Not found tabadmin executable!");
            }

            Process process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = tabadmin;
            process.StartInfo.Arguments = arguments;

            process.Start();

            /* tabadmin seems to only use stdout */
            string stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("tabadmin failed: " + arguments + ", ExitCode=" + Convert.ToString(process.ExitCode) + "\n" + stdout);
            }

            Log.Info("Output of tabadmin {0} command: '{1}'", arguments, stdout);
            return stdout;
        }

        public void enableReadonlyUser(string password)
        {
            tabadminRun("dbpass --username readonly " + password);
        }

        public static bool readOnlyEnabled(Dictionary<string, string> settings)
        {
            try
            {
                return Convert.ToBoolean(settings[Tableau.YML_READONLY_ENABLED]);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int getVersionNumber()
        {
            var tableauVersionStr = Environment.GetEnvironmentVariable("TABLEAU_SERVER_DATA_DIR_VERSION");
            Log.Info("Tableau version: '{0}'", tableauVersionStr);
            return getMajorVersion(tableauVersionStr);
        }

        private static int getMajorVersion(string versionStr)
        {
            if (versionStr == null || versionStr == "")
            {
                return 0;
            }

            try
            {
                Version ver = Version.Parse(versionStr);
                return ver.Major;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to parse Tableau version string: {0}! Exception:", versionStr);
            }

            return 0;
        }
    }
}
