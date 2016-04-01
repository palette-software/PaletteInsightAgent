using NLog;
using PaletteInsightAgent.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

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
                string maxId = null;
                if (IsStreamingTable(file))
                {
                    maxId = File.ReadAllText(MaxIdFileName(file));
                }
                return DoSendFile(file, maxId);
            });
        }

        public static string GetFileNameWithoutPart(string fileName)
        {
            var pattern = new Regex("(.*-[0-9]{4}-[0-9]{2}-[0-9]{2}--[0-9]{2}-[0-9]{2}-[0-9]{2})(.*)([.].+)$");
            return pattern.Replace(fileName, "$1$3");
        }

        private string MaxIdFileName(string fileName)
        {
            return SinglefileBackend.GetFileNameWithoutPart(fileName) + "maxid";
        }

        private bool IsStreamingTable(string fileName)
        {
            return File.Exists(MaxIdFileName(fileName));
        }


        public bool IsInProgress(string tableName)
        {
            return false;
        }

        // prepare array for results operation
        // this is needed so that we move the .maxid files as well as the csvs 
        private string[] PrepareResultArray(string file, string maxId)
        {
            string[] results;
            if (maxId != null)
            {
                results = new string[] { file, MaxIdFileName(file) };
            }
            else
            {
                results = new string[] { file };
            }
            return results;
        }

        /// <summary>
        /// Tries to send a list of files (or a single file) to the webservice.
        /// </summary>
        /// <param name="file">The name / path of the file </param>
        /// <param name="metadata">The contents of the metadata</param>
        /// <returns></returns>
        private OutputWriteResult DoSendFile(string file, string maxId)
        {
            var results = PrepareResultArray(file, maxId);

            // skip working if the file does not exist
            if (!File.Exists(file)) return OutputWriteResult.Failed(results);

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
                            return OutputWriteResult.Ok(results);
                        // On Md5 failiure re-send the file
                        case HttpStatusCode.Conflict:
                            Log.Warn("-> MD5 error in '{0}' -- resending", file);
                            return DoSendFile(file, maxId);
                        // otherwise move to the errored ones for now as that is the safe bet
                        // we should later handle some error codes differently so that upload is blocked until successful
                        // in certain cases
                        default:
                            Log.Error("-> Unknown status: '{0}' for '{1}' -- moving to error", result.StatusCode, file);
                            return OutputWriteResult.Failed(results);
                    }
                }
                catch (Exception e)
                {
                    // if we have an error here, that should mean
                    // we are having some HTTP errors (as this block in the recursive 
                    // DoSendFile() calls should catch any errors propagating from nested
                    // sends
                    Log.Error(e, "Error during sending '{0}' to the webservice: {1}", file, e);
                    // so we are just adding this file to the failed ones.
                    return OutputWriteResult.Failed(results);

                }
            });
        }
    }
}
