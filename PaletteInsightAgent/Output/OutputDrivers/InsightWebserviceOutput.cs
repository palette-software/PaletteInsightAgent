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


    /// <summary>
    /// Output class that writes to a web service
    /// </summary>
    public class WebserviceOutput : IOutput
    {
        private const string MANIFEST_KEY = "_manifest";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private WebserviceConfiguration config;
        private WebserviceErrorHandler errorHandler;

        /// <summary>
        /// The default encoding for the log files (we use this to read the bytes)
        /// </summary>
        private static Encoding defaultStringEncoding = new UTF8Encoding();

        /// <summary>
        /// The hash algorith to use for the signature
        /// </summary>
        private HashAlgorithm fileHasher = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5"));

        private WebserviceOutput(WebserviceConfiguration config, WebserviceErrorHandler errorHandler)
        {
            this.config = config;
            this.errorHandler = errorHandler;

            Log.Info("Initialized web service with endpoint '{0}' and user '{1}", config.Endpoint, config.Username);
        }

        /// <summary>
        /// Factory method for WebserviceOutput.
        /// This method is here because we dont want to throw in the constructor on config errors
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static WebserviceOutput MakeWebservice(WebserviceConfiguration config, WebserviceErrorHandler errorHandler)
        {
            if (!config.IsValid)
            {
                throw new ArgumentException("Invalid webservice configuration provided!");
            }
            return new WebserviceOutput(config, errorHandler);
        }


        #region IOutput

        public OutputWriteResult Write(IList<string> csvFiles)
        {
            // skip empty batches
            if (csvFiles.Count == 0) return new OutputWriteResult { };

            // try to send it as a pack
            ResponseResult batchSendResult = DoSendFileList(csvFiles);

            // if we have no files to retry, send the results straight back
            if (batchSendResult.FilesToRetry.Count == 0)
            {
                return batchSendResult.Result;
            }

            // otherwise retry the files we need to retry
            return OutputWriteResult.Aggregate(batchSendResult.FilesToRetry, (file) => {
                Log.Info("- Retrying file {0}", file);
                // do the sending, and ignore any files added to the retry list
                // as we are already in the retry phase
                var thisFileResult = DoSendFileList(new List<string> { file });
                // add the retry files to the unsent file list
                thisFileResult.Result.unsentFiles.AddRange(thisFileResult.FilesToRetry);
                return thisFileResult.Result;
            });
        }

        /// <summary>
        /// Tries to send a list of files (or a single file) to the webservice.
        /// </summary>
        /// <param name="csvFiles"></param>
        /// <param name="uploadUrl"></param>
        /// <returns></returns>
        private ResponseResult DoSendFileList(IList<string> csvFiles)
        {
            ResponseResult batchSendResult;
            using (var httpClient = new HttpClient())
            {
                var encoding = new ASCIIEncoding();
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoding.GetBytes(String.Format("{0}:{1}", config.Username, config.Password))));
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                // create the form data for upload
                MultipartFormDataContent form = new MultipartFormDataContent();
                // try to pack the list of files into the request
                var packResult = PackFilesIntoRequest(csvFiles, form);

                var uploadUrl = config.Endpoint + "/upload-many/testpkg";

                // try to send the files that packed successfully
                batchSendResult = DoSendToWebservice(packResult.PackedOk, uploadUrl, httpClient, form);
            }

            return batchSendResult;
        }

        /// <summary>
        /// Handler to send a list of files to the webservice and deal with the results
        /// </summary>
        /// <param name="csvFiles"></param>
        /// <param name="uploadUrl"></param>
        /// <param name="httpClient"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        private ResponseResult DoSendToWebservice(IList<string> csvFiles, string uploadUrl, HttpClient httpClient, MultipartFormDataContent form)
        {
            // create the return value
            var outputWriteResult = new OutputWriteResult { };

            return LoggingHelpers.TimedLog<ResponseResult>(Log, String.Format("Uploading to : {0}", uploadUrl), () =>
            {
                HttpResponseMessage result = null;
                string contentData = null;
                UploadManyResponse responseData = null;

                // try to send the request, convert the result from JSON
                try
                {
                    var response = httpClient.PostAsync(uploadUrl, form);
                    // gyulalaszlo: we use response.Wait() here, becuse VS2015 refused to compile await ... for me
                    response.Wait();

                    result = response.Result;
                    contentData = result.Content.ReadAsStringAsync().Result;
                    responseData = JsonConvert.DeserializeObject<UploadManyResponse>(contentData);
                    return errorHandler.OnSendResult(result, responseData, csvFiles);
                }
                catch (Exception e)
                {
                    // if any exceptions are thrown during the JSON parsing,
                    // we cannot determine if the batch suceeded or not, so we consider
                    // this recoverable-by-action, as we dont want to lose data, but we
                    // may not want to constantly re-upload this file to the server
                    return errorHandler.OnSendError(e, csvFiles, result);
                }
            });
        }

        /// <summary>
        /// The result of a packing operation (we need to integrate the results
        /// of packing into our checking cycle)
        /// </summary>
        class PackResult
        {
            /// <summary>
            /// A list of files that failed during the packing into the request.
            /// We need to remove these files from the checked files list, so
            /// we dont want to check them on return and fail because they werent
            /// in the request
            /// </summary>
            public List<string> FailedToPack = new List<string>();
            public List<string> PackedOk = new List<string>();
        }

        /// <summary>
        /// Packs all files in csvFiles into the multipart request specified by form.
        /// This method also adds a proper manifest to the request.
        /// </summary>
        /// <param name="csvFiles"></param>
        /// <param name="form"></param>
        private PackResult PackFilesIntoRequest(IList<string> csvFiles, MultipartFormDataContent form)
        {

            // the manifest we'll update
            var manifest = new UploadManifest { Files = new List<UploadInfo>() };
            var result = new PackResult { };


            // Go through each existing file
            foreach (var file in csvFiles.Where((f) => File.Exists(f)))
            {
                try
                {
                    // read all bytes here and use ByteArrayContents here, 
                    // because the HttpClients StreamContent
                    // needs the files to stay open until the request is submitted,
                    // and we would also need to open two streams, one for reading
                    // into the request, one for calculating the MD5
                    // TODO: benchmark this on large files to see if we can gain performance
                    var fileContent = File.ReadAllBytes(file);
                    // the key we'll use in the manifest
                    var fileBaseName = Path.GetFileName(file);

                    var manifestItem = new UploadInfo { Name = fileBaseName };

                    // hash to MD5 and add it to the current file's manifest
                    using (var md5 = MD5.Create())
                    {
                        manifestItem.Md5 = BitConverter.ToString(md5.ComputeHash(fileContent)).Replace("-", string.Empty);
                    }

                    Log.Debug("+ REQUEST: Adding file: '{0}' as '{1}' - {2} bytes", file, fileBaseName, fileContent.Length);

                    // add to the field
                    form.Add(new ByteArrayContent(fileContent), fileBaseName);

                    // add to the manifest list after everything was successful
                    manifest.Files.Add(manifestItem);

                    // add to the ok list
                    result.PackedOk.Add(file);
                }
                // If there is trouble, skip this file
                catch (Exception e)
                {
                    // notify the error handler.
                    // This error may mean that the files may have been renamed / moved,
                    // so if we return true, we add the file to the failed list
                    if (errorHandler.ErrorDuringPayloadCreation(file, e))
                    {
                        result.FailedToPack.Add(file);
                    } else {
                        // do nothing for now, but:
                        // there may come a time when this exception is needed.
                        //throw;
                    }
                    // continue with the next file
                }
            }


            // package a manifest with the files
            form.Add(new StringContent(JsonConvert.SerializeObject(manifest)), MANIFEST_KEY);

            return result;
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// Since this output driver holds no resources that need to be released,
        /// we dont do anything is Dispose()
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

    }
}
