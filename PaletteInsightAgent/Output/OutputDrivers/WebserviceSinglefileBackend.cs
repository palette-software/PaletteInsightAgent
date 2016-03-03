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
    /// <summary>
    /// A webservice backend for dealing with single file uploads
    /// </summary>
    public class SinglefileBackend : WebserviceBackendBase, WebserviceBackend
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Gets the endpoint url for a file upload.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="filename"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        private string GetEndpointUrl(string package)
        {
            return String.Format("{0}/upload-with-meta?pkg={1}", config.Endpoint, package );
        }


        public OutputWriteResult Write(IList<string> csvFiles)
        {
            // skip empty batches
            if (csvFiles.Count == 0) return new OutputWriteResult { };

            // otherwise retry the files we need to retry
            return OutputWriteResult.Aggregate(csvFiles, (file) =>
            {
                Log.Info("+ Sending file {0}", file);
                return DoSendFile(file);
            });
        }


        /// <summary>
        /// Tries to send a list of files (or a single file) to the webservice.
        /// </summary>
        /// <param name="file">The name / path of the file </param>
        /// <param name="metadata">The contents of the metadata</param>
        /// <param name="package">the name of the package this </param>
        /// <returns></returns>
        private OutputWriteResult DoSendFile(string file, string metadata = "", string package = "public")
        {
            // skip working if the file does not exist
            if (!File.Exists(file)) return OutputWriteResult.Failed(file);

            return WithConnection((httpClient) =>
            {


                // get the endpoint
                var uploadUrl = GetEndpointUrl(package);

                return LoggingHelpers.TimedLog<OutputWriteResult>(Log, String.Format("Uploading to : {0}", uploadUrl), () =>
                {
                    // try to send the request, convert the result from JSON
                    try
                    {
                        // send the request as a file stream
                        var response = httpClient.PostAsync(uploadUrl, CreateRequestContents(file, metadata));
                        // gyulalaszlo: we use response.Wait() here, becuse VS2015 refused to compile await ... for me
                        response.Wait();

                        var result = response.Result;
                        switch (result.StatusCode)
                        {
                            // if we are ok, we are ok
                            case HttpStatusCode.OK:
                                Log.Debug("-> Sent ok: '{0}'", file);
                                return OutputWriteResult.Ok(file);
                            // On Md5 failiure re-send the file
                            case HttpStatusCode.Conflict:
                                Log.Warn("-> MD5 error in '{0}' -- resending", file);
                                return DoSendFile(file);
                            // otherwise move to the unsent ones, as this most likely is
                            // a server or auth error
                            default:
                                Log.Warn("-> Unknown status: '{0}' for '{1}' -- moving to unsent", result.StatusCode, file);
                                return OutputWriteResult.Unsent(file);
                        }
                    }
                    catch (Exception e)
                    {
                        // if we have an error here, that should mean
                        // we are having some HTTP errors (as this block in the recursive 
                        // DoSendFile() calls should catch any errors propagating from nested
                        // sends
                        Log.Error(e, "Error during sending '{0}' to the webservice: {1}", file, e);
                        // so we are just adding this file to the unsent ones.
                        return OutputWriteResult.Unsent(file);

                    }
                });
            });
        }

        private static MultipartFormDataContent CreateRequestContents(string file, string metadata)
        {
            // create the form data for upload
            MultipartFormDataContent form = new MultipartFormDataContent();
            // try to pack the list of files into the request

            var fileContent = File.ReadAllBytes(file);
            var fileBaseName = Path.GetFileName(file);

            byte[] fileHash;
            using (var md5Hasher = MD5.Create())
            {
                fileHash = md5Hasher.ComputeHash(fileContent);
            }

            Log.Debug("+ REQUEST: Adding file: '{0}' as '{1}' - {2} bytes", file, fileBaseName, fileContent.Length);

            // add to the fields
            form.Add(new ByteArrayContent(fileContent), "_file", fileBaseName);
            // we encode the md5 as a base64 value, so we dont have to do tricks on
            // go's side to get this value
            form.Add(new StringContent(Convert.ToBase64String(fileHash)), "_md5");
            form.Add(new StringContent(metadata), "_meta", String.Format("{0}.meta", fileBaseName));
            return form;
        }
    }
}
