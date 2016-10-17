﻿using NLog;
using PaletteInsightAgent.Helpers;
using System;
using System.IO;

namespace PaletteInsightAgent.Output.OutputDrivers
{
    /// <summary>
    /// A webservice backend for dealing with single file uploads
    /// </summary>
    public class SinglefileBackend : IOutput
    {
        public WebserviceConfiguration config;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Write(string file)
        {
            Log.Info("+ Sending file {0}", file);
            string maxId = null;
            if (FileUploader.IsStreamingTable(file))
            {
                try
                {
                    maxId = File.ReadAllText(FileUploader.MaxIdFileName(file));
                }
                catch (UnauthorizedAccessException)
                {
                    throw new TemporaryException("Unauthorized access or file is being used by another process.");
                }
            }
            DoSendFile(file, maxId);
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
                results = new string[] { file, FileUploader.MaxIdFileName(file) };
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
        private void DoSendFile(string file, string maxId)
        {
            var results = PrepareResultArray(file, maxId);

            // skip working if the file does not exist
            if (!File.Exists(file)) return; 

            LoggingHelpers.TimedLog(Log, String.Format("Uploading file : {0}", file), () =>
            {
                // try to send the request, convert the result from JSON
                var uploadTask = APIClient.UploadFile(file, maxId);
                // Wait for the result, so that our timed log can actually measure something
                uploadTask.Wait();

                Log.Debug("-> Sent ok: '{0}'", file);
                return;
            });
        }

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
