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
        /// The authentication username for the webservice
        /// </summary>
        public string Username;
        public string Password;

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
                return Endpoint.Length >= 4 && Endpoint.StartsWith("http") && Username.Length > 0 && Password.Length > 0;
            }
        }
    }

    #region REST API Models


    /// <summary>
    /// An element for a single file in the manifest
    /// </summary>
    public class UploadInfo
    {
        public string Name { get; set; }
        public string Md5 { get; set; }
    }

    /// <summary>
    /// The manifest packed with each multi-upload
    /// </summary>
    public class UploadManifest
    {
        public List<UploadInfo> Files { get; set; }
    }

    /// <summary>
    /// The servers response to a single uploaded file.
    /// </summary>
    public class UploadResponse
    {
        public string Name { get; set; }
        public string Md5 { get; set; }

        public string UploadPath { get; set; }
        public DateTime UploadTime { get; set; }

        public int Status { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// The response to an upload-many request
    /// </summary>
    public class UploadManyResponse
    {
        public List<UploadResponse> Files;
    }

    #endregion

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
        public static IOutput MakeWebservice(WebserviceConfiguration config, WebserviceErrorHandler errorHandler)
        {
            if (!config.IsValid)
            {
                throw new ArgumentException("Invalid webservice configuration provided!");
            }
            // select which endpoint to use
            if (config.UseMultifile)
                return new MultifileBackend { config = config, errorHandler = errorHandler };
            else
                return new SinglefileBackend { config = config, errorHandler = errorHandler };
        }
    }
}
