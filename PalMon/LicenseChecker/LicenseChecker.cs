using Licensing;
using NLog;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
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
                        Log.Info("Found valid license in {0}", f);
                        Log.Info("  - licensed to: {0}", license.owner);
                        Log.Info("  - licensed id is: {0}", license.licenseId);
                        Log.Info("  - license core count: {0}", license.coreCount);
                        Log.Info("  - valid until: {0}", license.validUntilUTC.ToLongDateString());
                        return true;
                    } else
                    {
                        Log.Info("License is invalid.");
                    }

                }
                catch (Exception e)
                {
                    Log.Fatal("Error in LicenseChecker:", e);
                }
            }

            Log.Info("No valid license found");
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
                    cmd.CommandText = "SELECT coalesce(sum(allocated_cores),0) FROM core_licenses;";
                    long coreCount = (long)cmd.ExecuteScalar();
                    Log.Info("Tableau total allocated cores: {0}", coreCount);
                    return (int)coreCount;
                }
            }
        }
    }
}
