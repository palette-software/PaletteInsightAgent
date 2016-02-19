using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PaletteInsightAgent.Output
{
    // CSV Folder:
    // serverlog-2016-01-28-15-06-00.csv
    // serverlog-2016-01-28-15-06-30.csv
    // threadinfo-2016-01-28-15-06-00.csv

    class DBWriter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string CSV_PATH = @"csv/";
        private const string CSV_PATTERN = @"*.csv";
        /// <summary>
        /// The directory we store the succesfully uploaded files
        /// </summary>
        private const string PROCESSED_PATH = @"csv/processed/";
        /// <summary>
        /// The directory where the files that have errors (invalid names, etc.)
        /// </summary>
        private const string ERROR_PATH = @"csv/errors/";
        /// <summary>
        /// The path where files to be re-sent later are stored.
        /// TODO: try these files on start
        /// </summary>
        private const string UNSENT_PATH = @"csv/unsent/";

        /// <summary>
        /// A list of table names we actually care about.
        /// </summary>
        private static readonly List<string> TABLE_NAMES = new List<string> { "countersamples", "serverlogs", "threadinfo", "filter_state_audit" };

        public  static readonly object DBWriteLock = new object();
        private static readonly int waitLockTimeout = 1000;

        /// <summary>
        /// Helper method that is usually called on startup to check the unsent folder
        /// for unsent files and tries to send them
        /// </summary>
        /// <param name="output"></param>
        public static void TryToSendUnsentFiles(IOutput output)
        {
            DoUpload(output, UNSENT_PATH);
        }

        /// <summary>
        /// Start a single write loop
        /// </summary>
        /// <param name="output"></param>
        public static void Start(IOutput output)
        {
            DoUpload(output, CSV_PATH);
        }

        /// <summary>
        /// Implementation of sending all CSV files from a directory
        /// </summary>
        /// <param name="output"></param>
        /// <param name="csvPath"></param>
        private static void DoUpload(IOutput output, string csvPath)
        {
            if (!Monitor.TryEnter(DBWriteLock, waitLockTimeout))
            {
                Log.Debug("Skipping DB write as it is already in progress.");
                return;
            }

            try
            {
                IList<string> fileList;

                // The old code (a while loop) gets stuck if we use the 'unsent' folder
                // as a source.
                MoveAllFiles(OutputWriteResult.Aggregate( TABLE_NAMES,(table) => {
                    return output.Write(GetFilesOfTable(csvPath, table));
                }));
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to write CSV files to database! Exception message: {0}", e);
            }
            finally
            {
                Monitor.Exit(DBWriteLock);
            }
        }

        private static void MoveAllFiles(OutputWriteResult processedFiles)
        {
            // Move files to processed folder
            MoveToFolder(processedFiles.successfullyWrittenFiles, PROCESSED_PATH);
            // Move files with errors to the errors folder
            MoveToFolder(processedFiles.failedFiles, ERROR_PATH);
            // Move files with errors to the errors folder
            MoveToFolder(processedFiles.unsentFiles, UNSENT_PATH);
        }

        /// <summary>
        /// Gets all CSV files from a csvPath with the prefix specified by table.
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static IList<string> GetFilesOfTable(string csvPath, string table)
        {
            // Remove those files that are still being written.
            return Directory.GetFiles(csvPath, table + "-" + CSV_PATTERN)
                            .Where(fileName => !fileName.Contains(CsvOutput.IN_PROGRESS_FILE_POSTFIX))
                            .ToList();
        }

        #region Deprecated, but used in tests

        /// <summary>
        /// DbWriterTests uses this function with this signature.
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetFilesOfSameTable()
        {
            return GetFilesOfSameTable(CSV_PATH);
        }

        /// <summary>
        /// Gives back a list of files for the same table, empty list otherwise 
        /// </summary>
        /// <param name="csvPath">The directory of CSV files</param>
        /// <returns></returns>
        public static IList<string> GetFilesOfSameTable(string csvPath)
        {
            var allFiles = Directory.GetFiles(csvPath, CSV_PATTERN);
            if (allFiles.Length == 0)
            {
                return new List<string>();
            }
            var pattern = GetTableName(allFiles[0]);
            if (pattern == "")
            {
                return new List<string>();
            }

            // Remove those files that are still being written.
            return Directory.GetFiles(csvPath, pattern + "-" + CSV_PATTERN)
                            .Where(fileName => !fileName.Contains(CsvOutput.IN_PROGRESS_FILE_POSTFIX))
                            .ToList();
        }


        public static string GetFileName(string fullFileName)
        {
            var tokens = fullFileName.Split('/');
            if (tokens.Length == 0)
            {
                return "";           
            }
            return tokens[tokens.Length - 1];
        }

        // FileName: tableName-otherstuff.ext
        // Return tableName
        // If FileName is not in the given format return ""
        public static string GetTableName(string fullFileName)
        {
            var fileName = GetFileName(fullFileName);
            var tokens = fileName.Split('-');
            if (tokens.Length == 1)
            {
                // If the file name does not contain any delimiters,
                // then it must be some error since we are creating
                // these files.
                Log.Error("Failed to extract table name from file name: {0}", fullFileName);
                return "";
            }
            return tokens[0];
        }

        #endregion

        /// <summary>
        /// Helper method to move a list of files to a new folder
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="outputFolder"></param>
        private static void MoveToFolder(IList<string> fileList, string outputFolder)
        {
            foreach (var fullFileName in fileList)
            {
                try
                {
                    // create the output directory
                    if (!Directory.Exists(outputFolder))
                        Directory.CreateDirectory(outputFolder);

                    var fileName = GetFileName(fullFileName);
                    Log.Debug("Trying to move: {0}", fileName);
                    var targetFile = Path.Combine(outputFolder, fileName);

                    // If we are trying to move from unsent to unsent we may find
                    // that we have the same filename twice
                    if (Path.GetFullPath(targetFile) == Path.GetFullPath(fullFileName))
                    {
                        Log.Debug("Skipping moving a file to itself: {0}", fullFileName);
                        continue;
                    }
                    
                    // Delete the output if it already exists
                    // TODO: shouldnt we rename the old file here?
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);

                    // Do the actual move
                    File.Move(fullFileName, targetFile);
                    Log.Info("Processed file: {0}", fileName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception while moving file {0} to {1}: {2}", fullFileName, outputFolder, ex);
                }
            }
        }

        internal static void MoveToProcessed(IList<string> testFileList)
        {
            MoveToFolder(testFileList, PROCESSED_PATH);
        }

    }
}
