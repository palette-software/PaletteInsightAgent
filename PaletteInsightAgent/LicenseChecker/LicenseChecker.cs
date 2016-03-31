using Licensing;
using NLog;
using System;
using System.IO;

namespace PaletteInsightAgent.LicenseChecker
{
    class LicenseChecker
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        const string LICENSE_FILE_PATTERN = "*.license";

        /// <summary>
        /// Check for a valid license file in baseDirectory
        /// </summary>
        /// <param name="baseDirectory">The directory to look for .license files</param>
        /// <param name="publicKey">The public key to validate the license against</param>
        /// <param name="coreCount">The number of cores (if the license has less then this, the license is invalid)</param>
        /// <returns>true if there is a valid license or false if there is none</returns>
        public static License checkForLicensesIn(string baseDirectory, byte[] publicKey, int coreCount)
        {
            var licenseManager = new Ed25519LicenseManager();

            Log.Info("Trying to find a valid license for {0} cores in {1} with pattern: {2}", coreCount, baseDirectory, LICENSE_FILE_PATTERN);

            // check the current directory and subdirectories for license files
            var licenseFiles = Directory.GetFiles(baseDirectory, LICENSE_FILE_PATTERN, SearchOption.AllDirectories);

            foreach (var f in licenseFiles)
            {
                Log.Info("Checking license in: {0}", f);
                try
                {
                    var contents = File.ReadAllText(f);
                    var validLicense = licenseManager.isValidLicense(contents, coreCount, publicKey);
                    // If the license is valid, then return true and log the success and license info
                    if (validLicense.isValid)
                    {
                        var license = validLicense.license;

                        // Set a custom variable for NLog, so that we can filter on it in LogEntries
                        NLog.GlobalDiagnosticsContext.Set("license_owner", license.owner);

                        Log.Info("Found valid license in {0}", f);
                        Log.Info("  - licensed to: {0}", license.owner);
                        Log.Info("  - licensed id is: {0}", license.licenseId);
                        Log.Info("  - license core count: {0}", license.coreCount);
                        Log.Info("  - valid until: {0}", license.validUntilUTC.ToLongDateString());
                        return license;
                    } else
                    {
                        var license = validLicense.license;

                        Log.Info("Invalid license in {0}", f);
                        Log.Info("  - licensed to: {0}", license.owner);
                        Log.Info("  - licensed id is: {0}", license.licenseId);
                        Log.Info("  - license core count: {0}", license.coreCount);
                        Log.Info("  - valid until: {0}", license.validUntilUTC.ToLongDateString());
                        Log.Info("License is invalid.");
                    }

                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Error in LicenseChecker: {0}", e);
                }
            }

            Log.Info("No valid license found");
            // No valid licenses found
            return null;
        }
    }
}
