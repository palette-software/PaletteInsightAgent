using Licensing;
using log4net;
using Npgsql;
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
        /// <param name="coreCount">The number of cores (if the license has less then this, the license is invalid)</param>
        /// <returns>true if there is a valid license or false if there is none</returns>
        public static bool checkForLicensesIn(string baseDirectory, byte[] publicKey, int coreCount)
        {
            var licenseManager = new Ed25519LicenseManager();

            Log.DebugFormat("Trying to find a valid license for {0} cores", coreCount);

            // check the current directory and subdirectories for license files
            var licenseFiles = Directory.GetFiles(baseDirectory, LICENSE_FILE_PATTERN, SearchOption.AllDirectories);
            foreach (var file in licenseFiles)
            {
                Log.DebugFormat("Checking license in: {0}", file);
                var contents = File.ReadAllText(file);
                var validLicense = licenseManager.isValidLicense(contents, coreCount, publicKey);
                // If the license is valid, then return true and log the success and license info
                if (validLicense.isValid)
                {
                    var license = validLicense.license;
                    Log.InfoFormat("Found valid license in {0}", file);
                    Log.InfoFormat("  - licensed to: {0}", license.owner);
                    Log.InfoFormat("  - licensed id is: {0}", license.licenseId);
                    Log.InfoFormat("  - license core count: {0}", license.coreCount);
                    Log.InfoFormat("  - valid until: {0}", license.validUntilUTC.ToLongDateString());
                    return true;
                }
            }

            Log.Debug("No valid license found");
            // No valid licenses found
            return false;
        }


        /// <summary>
        /// Returns the total number of cores allocated to the Tableau cluster represented by the repository.
        /// </summary>
        public static int getCoreCount(string repoHost, int repoPort, string repoUser, string repoPass, string repoDb)
        {
            // The postgres connection string
            var connectionString = String.Format("Host={0};Port={1};Username={2};Password={3};Database={4}",
                repoHost, repoPort, repoUser, repoPass, repoDb);

            // connect to the repo
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    // Insert some data
                    cmd.CommandText = "SELECT sum(allocated_cores) FROM core_licenses;";
                    int coreCount = (int)cmd.ExecuteScalar();
                    Log.InfoFormat("Tableau total allocated cores: {0}", coreCount);
                    return coreCount;
                }
            }
        }
    }
}
