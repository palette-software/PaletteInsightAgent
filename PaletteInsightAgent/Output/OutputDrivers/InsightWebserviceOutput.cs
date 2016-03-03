using Newtonsoft.Json;
using NLog;
using PaletteInsightAgent.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output.OutputDrivers
{
    public class WebserviceConfiguration
    {
        /// <summary>
        /// The http/s endpoint to connect to.
        /// Example: https://user:pass@localhost:9000
        /// </summary>
        public string Endpoint;

        /// <summary>
        /// The authentication username for the webservice (the licenseId of the license)
        /// </summary>
        public string Username;

        /// <summary>
        /// The authentication token from the license
        /// </summary>
        public byte[] AuthToken;

        /// <summary>
        /// Should the output use multiple-file-upload (true) or single-file-upload(false)
        /// </summary>
        public bool UseMultifile = false;

        /// <summary>
        /// Returns true if the webservice configuration is valid.
        /// TODO: do a proper check of the Endpoint
        /// </summary>
        /// <returns></returns>
        public bool IsValid
        {
            get
            {
                return Endpoint.Length >= 4 && Endpoint.StartsWith("http") && Username.Length > 0 && AuthToken.Length > 0;
            }
        }
    }
    public interface WebserviceBackend : IOutput
    {

    }



    /// <summary>
    /// Output class that writes to a web service
    /// </summary>
    public class WebserviceOutput
    {

        /// <summary>
        /// Factory method for WebserviceOutput.
        /// This method is here because we dont want to throw in the constructor on config errors
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IOutput MakeWebservice(WebserviceConfiguration config)
        {
            if (!config.IsValid)
            {
                throw new ArgumentException("Invalid webservice configuration provided!");
            }
            return new SinglefileBackend { config = config };
        }
    }
}
