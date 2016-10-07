using System;
using System.Collections.Generic;
using NLog;
using PaletteInsightAgent.Helpers;

namespace PaletteInsightAgent.License
{
    class LicenseGuard
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly int MAX_ALLOWED_LICENSE_FAILURES = 3;
        private int licenseFailureCount;

        /// <summary>
        /// Validates the specified license key via the Insight Server. Any
        /// communication error with the Insight Server will cause the
        /// check to fail.
        /// </summary>
        /// <param name="licenseKey">
        /// License key to check.
        /// </param>
        /// <returns></returns>
        public bool CheckLicense(string licenseKey)
        {
            try
            {
                var licensePromise = APIClient.CheckLicense(licenseKey);
                licensePromise.Wait();
                string checkJsonString = licensePromise.Result;

                Dictionary<string, object> parsedResult;

                parsedResult = (Dictionary<string, object>)fastJSON.JSON.Parse(checkJsonString);

                // Set a custom variable for NLog, so that we can filter on it in LogEntries
                NLog.GlobalDiagnosticsContext.Set("license_owner", parsedResult["owner"]);

                string licenseMessage = String.Format("Owner: {0} -- valid until: {1}", parsedResult["owner"], parsedResult["expiration-time"]);

                if (Convert.ToBoolean(parsedResult["valid"]))
                {
                    Log.Info("License key is valid. {0}", licenseMessage);
                    return true;
                }

                Log.Error("License key {0} is invalid! {1}", licenseKey, licenseMessage);
                return false;
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    Log.Warn(x, "Failed to validate license! Error:");

                    // This return does not mean that the license is OK, but to tell that
                    // the aggreagated exception is handled.
                    return true;
                });
            }
            catch (Exception e)
            {
                Log.Warn(e, "License check failed! Error:");
            }

            return false;
        }

        /// <summary>
        /// Timer invokable license check function
        /// </summary>
        /// <param name="stateInfo">
        /// The license key passed as a string.
        /// </param>
        public void PollLicense(object stateInfo)
        {
            string licenseKey = (string)stateInfo;
            if (CheckLicense(licenseKey))
            {
                licenseFailureCount = 0;
                return;
            }

            if (++licenseFailureCount >= MAX_ALLOWED_LICENSE_FAILURES)
            {
                // The grace period is 3 days. It means that there 3 days to fix the license or
                // the connection between the Insight Agent and the Insight Server.
                Log.Fatal("License check failed {0} times in-a-row! Exiting...", MAX_ALLOWED_LICENSE_FAILURES);
                Environment.Exit(-1);
            }
        }
    }
}
