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
        private const string csvPath = @"csv/";
        /// <summary>
        /// The directory we store the succesfully uploaded files
        /// </summary>
        private const string PROCESSED_PATH = @"csv/processed/";
        /// <summary>
        /// The directory where the files that have errors (invalid names, etc.)
        /// </summary>
        private const string ERROR_PATH = @"csv/errors/";
        private const string csvPattern = @"*.csv";

        public  static readonly object DBWriteLock = new object();
        private static readonly int waitLockTimeout = 1000;

        public static void Start(IOutput output)
        {
            if (!Monitor.TryEnter(DBWriteLock, waitLockTimeout))
            {
                Log.Debug("Skipping DB write as it is already in progress.");
                return;
            }

            try
            {
                IList<string> fileList;
                while ((fileList = GetFilesOfSameTable()).Count > 0)
                {
                    // BULK COPY
                    // we return the list of actual files we have successfully processed
                    var processedFiles = output.Write(fileList);
                    // Move files to processed folder
                    MoveToFolder(processedFiles.successfullyWrittenFiles, PROCESSED_PATH);
                    // Move files with errors to the errors folder
                    MoveToFolder(processedFiles.failedFiles, ERROR_PATH);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // This means that the CSV folder does not exist, which also means that
                // there are no CSV files to process.
            }
            catch (Exception e)
            {
                Log.Error("Failed to write CSV files to database! Exception message: {0}", e.Message);
            }
            finally
            {
                Monitor.Exit(DBWriteLock);
            }
        }


        // Gives back a list of files for the same table, empty list otherwise
        public static IList<string> GetFilesOfSameTable()
        {
            var allFiles = Directory.GetFiles(csvPath, csvPattern);
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
            return Directory.GetFiles(csvPath, pattern + "-" + csvPattern)
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
