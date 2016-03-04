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

        public OutputWriteResult Write(IList<string> csvFiles)
        {
            // skip empty batches
            if (csvFiles.Count == 0) return new OutputWriteResult { };

            // otherwise retry the files we need to retry
            return OutputWriteResult.Aggregate(csvFiles, (file) =>
            {
                Log.Info("+ Sending file {0}", file);
                bool streaming = File.Exists(file + "maxid");
                string maxId = null;
                if (streaming)
                {
                    maxId = File.ReadAllText(file + "maxid");
                }
                return DoSendFile(file, maxId);
            });
        }


        public bool IsInProgress(string tableName)
        {
            return false;
        }

        /// <summary>
        /// Tries to send a list of files (or a single file) to the webservice.
        /// </summary>
        /// <param name="file">The name / path of the file </param>
        /// <param name="metadata">The contents of the metadata</param>
        /// <returns></returns>
        private OutputWriteResult DoSendFile(string file, string maxId)
        {
            // skip working if the file does not exist
            if (!File.Exists(file)) return OutputWriteResult.Failed(file);

            return LoggingHelpers.TimedLog<OutputWriteResult>(Log, String.Format("Uploading file : {0}", file), () =>
            {
                // try to send the request, convert the result from JSON
                try
                {
                    var response = APIClient.UploadFile(file, maxId);
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
                            return DoSendFile(file, maxId);
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
        }
    }
}
