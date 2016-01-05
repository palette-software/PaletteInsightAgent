using Licensing;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.LicenseChecker
{
    class LicenseChecker
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        const string LICENSE_FILE_PATTERN = "*.license";

        /// <summary>
        /// Check for a valid license file in baseDirectory
        /// </summary>
        /// <param name="baseDirectory">The directory to look for .license files</param>
        /// <param name="publicKey">The public key to validate the license against</param>
        /// <returns>true if there is a valid license or false if there is none</returns>
        public static bool checkForLicensesIn(string baseDirectory, byte[] publicKey)
        {
            var licenseManager = new Ed25519LicenseManager();

            // check the current directory and subdirectories for license files
            var licenseFiles = Directory.GetFiles(baseDirectory, LICENSE_FILE_PATTERN, SearchOption.AllDirectories);
            foreach (var file in licenseFiles)
            {
                Log.DebugFormat("Checking license in: {0}", file);
                var contents = File.ReadAllText(file);
                var validLicense = licenseManager.isValidLicense(contents, publicKey);
                // If the license is valid, then return true and log the success and license info
                if (validLicense.isValid)
                {
                    var license = validLicense.license;
                    Log.InfoFormat("Found valid license in {0}", file);
                    Log.InfoFormat("  - licensed to: {0}", license.owner);
                    Log.InfoFormat("  - licensed id is: {0}", license.licenseId);
                    Log.InfoFormat("  - valid until: {0}", license.validUntilUTC.ToLongDateString());
                    return true;
                }
            }

            // No valid licenses found
            return false;
        }
    }
}
