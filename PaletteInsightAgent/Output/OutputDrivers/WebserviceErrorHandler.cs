using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output.OutputDrivers
{

    /// <summary>
    /// Error handler interface for the web service output
    /// </summary>
    public interface WebserviceErrorHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="e"></param>
        /// <returns>True if the file needs to be skipped, false if the exception needs to be thrown (thus halting the request</returns>
        bool ErrorDuringPayloadCreation(string file, Exception e);

        /// <summary>
        /// When sending a request was successful, this function checks the result.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="responseData"></param>
        /// <param name="csvFiles"></param>
        /// <returns></returns>
        ResponseResult OnSendResult(HttpResponseMessage result, UploadManyResponse responseData, IList<string> csvFiles);

        /// <summary>
        /// If we have a send error, we consider that a resolvable-by-action
        /// </summary>
        /// <param name="e"></param>
        /// <param name="csvFilesCopy"></param>
        /// <returns></returns>
        ResponseResult OnSendError(Exception e, IList<string> list, HttpResponseMessage result);
    }


    /// <summary>
    /// </summary>
    public class ResponseResult
    {
        public OutputWriteResult Result = new OutputWriteResult();
        /// <summary>
        /// A list of files to retry after the batch try.
        /// </summary>
        public List<string> FilesToRetry = new List<string>();
    }

    /// <summary>
    /// A basic error handler implementation
    /// </summary>
    public class BasicErrorHandler : WebserviceErrorHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public bool ErrorDuringPayloadCreation(string file, Exception e)
        {
            if (e is System.IO.FileNotFoundException)
            {
                Log.Error(e, "Cannot find file: {0}", file);
                return true;
            }
            // otherwise log the error
            Log.Error(e, "Error during adding '{0}' to request payload: {1}", file, e);
            // for now, we signal that the file needs to be removed
            // TODO: check if there are other cases when we dont want to remove the file
            return false;
        }

        /// <summary>
        /// If we have a send error, we consider that a resolvable-by-action
        /// </summary>
        /// <param name="e"></param>
        /// <param name="csvFilesCopy"></param>
        /// <returns></returns>
        public ResponseResult OnSendError(Exception e, IList<string> csvFiles, HttpResponseMessage result)
        {
            // errors during an HTTP request generally indicate that we have
            // some transport errors, so we can re-send the files later, but should not retry in this round

            // log the error
            if (result == null)
            {
                Log.Error(e, "Error HTTP request, during sending: {0}", e);
            }
            else
            {
                Log.Error(e, "Error during sending: STATUS: {0} CONTENTS:{1} ERROR:{2}", result.StatusCode, result, e);
            }

            // mark this batch as unsent files for now
            return new ResponseResult
            {
                Result = new OutputWriteResult { unsentFiles = new List<string>(csvFiles) },
            };
        }

        /// <summary>
        /// When sending a request was successful, this function checks the result.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="responseData"></param>
        /// <param name="csvFiles"></param>
        /// <returns></returns>
        public ResponseResult OnSendResult(HttpResponseMessage result, UploadManyResponse responseData, IList<string> csvFiles)
        {
            var statusCode = result.StatusCode;
            // Server errors result in files that we should not retry for now.
            if (statusCode != System.Net.HttpStatusCode.OK)
            {
                Log.Info("[WebService] Bad response code: {0}, moving files {1} to unsent ones.", statusCode, csvFiles);
                // return the proper response
                return new ResponseResult
                {
                    Result = new OutputWriteResult { unsentFiles = new List<string>(csvFiles) },
                };
            }

            // convert the file list to a dictionary so we can look it up
            var checkSet = responseData.Files.ToDictionary((f) => f.Name, (f) => f);

            var output = new ResponseResult { };
            foreach (var fileInCsvFolder in csvFiles)
            {
                // we need to use the basename of the file (file at this
                // point is csv/... so we need to remove the csv/ prefix)
                var file = Path.GetFileName(fileInCsvFolder);
                // if the file is not in the response manifest, its definitely unsent
                if (!checkSet.ContainsKey(file))
                {
                    output.Result.unsentFiles.Add(fileInCsvFolder);
                    continue;
                }

                var fileStatus = checkSet[file];

                switch (fileStatus.Status)
                {
                    // OK
                    case 200:
                        output.Result.successfullyWrittenFiles.Add(fileInCsvFolder);
                        break;

                    // MD5 error can be recovered by re-sending the file
                    case 409:
                        output.FilesToRetry.Add(fileInCsvFolder);
                        break;

                    // otherwise its an unsent file for now
                    default:
                        output.Result.unsentFiles.Add(fileInCsvFolder);
                        break;
                }
            }
            // now we can return the output
            return output;
        }
    }

}
