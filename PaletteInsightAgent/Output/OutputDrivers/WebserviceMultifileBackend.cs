using Newtonsoft.Json;
using NLog;
using PaletteInsightAgent.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output.OutputDrivers
{
    /// <summary>
    /// A webservice backend for dealing with multiple file uploads
    /// </summary>
    public class MultifileBackend : WebserviceBackendBase, WebserviceBackend
    {
        private const string MANIFEST_KEY = "_manifest";
        private const string UPLOAD_MULTI_ENDPOINT = "/upload-many/testpkg";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


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
            return OutputWriteResult.Aggregate(batchSendResult.FilesToRetry, (file) =>
            {
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
            return WithConnection((httpClient) =>
            {
                // create the form data for upload
                MultipartFormDataContent form = new MultipartFormDataContent();
                // try to pack the list of files into the request
                var packResult = PackFilesIntoRequest(csvFiles, form);
                var uploadUrl = config.Endpoint + UPLOAD_MULTI_ENDPOINT;

                // try to send the files that packed successfully
                return DoSendToWebservice(packResult.PackedOk, uploadUrl, httpClient, form);
            });
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
                    }
                    else {
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

    }
}
