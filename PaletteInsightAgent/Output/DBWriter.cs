using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PaletteInsightAgent.Output
{
    class DBWriter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The directory we store the succesfully uploaded files
        /// </summary>
        private const string PROCESSED_PREFIX = @"processed/";
        /// <summary>
        /// The directory where the files that have errors (invalid names, etc.)
        /// </summary>
        private const string ERROR_PREFIX = @"errors/";
        /// <summary>
        /// The path where files to be re-sent later are stored.
        /// TODO: try these files on start
        /// </summary>
        private const string UNSENT_PREFIX = @"unsent/";

        /// <summary>
        /// The chance (as fraction) that a call to Start() will also result in a call
        /// to TryToSendUnsentFiles()
        /// </summary>
        private const double UNSENT_FILES_RESEND_CHANCE = 0.01;

        private static string DataFilePattern
        {
            get
            {
                return "*" + OutputSerializer.Extension;
            }
        }

        private static string ProcessedPath
        {
            get
            {
                return Path.Combine(OutputSerializer.DATA_FOLDER, PROCESSED_PREFIX);
            }
        }

        private static string ErrorPath
        {
            get
            {
                return Path.Combine(OutputSerializer.DATA_FOLDER, ERROR_PREFIX);
            }
        }

        private static string UnsentPath
        {
            get
            {
                return Path.Combine(OutputSerializer.DATA_FOLDER, UNSENT_PREFIX);
            }
        }
        public static readonly object DBWriteLock = new object();
        private static readonly int waitLockTimeout = 1000;

        /// <summary>
        /// Helper method that is usually called on startup to check the unsent folder
        /// for unsent files and tries to send them
        /// </summary>
        /// <param name="output"></param>
        public static void TryToSendUnsentFiles(IOutput output)
        {
            DoUpload(output, UnsentPath);
        }

        /// <summary>
        /// Start a single write loop
        /// </summary>
        /// <param name="output"></param>
        public static void Start(IOutput output, int processedFilesTTL, long storageLimit)
        {
            // add some chance (1%) of uploading the unsent files, so once every
            // ~1500 seconds on average we try to re-upload the stuff we may have missed
            if (ShouldTryResendingData())
            {
                Log.Info("+++ LUCKY DRAW: trying to re-send unsent files +++");
                TryToSendUnsentFiles(output);
                Log.Info("+++ /LUCKY DRAW: done trying to re-sending unsent files +++");
            }
            DoUpload(output, OutputSerializer.DATA_FOLDER);
            DeleteOldFiles(ProcessedPath, processedFilesTTL);
            ApplyStorageLimit(storageLimit);
        }

        private static void DeleteOldFiles(string from, int ttl)
        {
            Directory.EnumerateFiles(from)
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < DateTime.Now.AddSeconds(-ttl))
                .ToList()
                .ForEach(f => f.Delete());
        }

        public static void ApplyStorageLimit(long storageLimit)
        {
            // FIXME: Do these checks in a more proper place
            if (!Directory.Exists(ProcessedPath))
                Directory.CreateDirectory(ProcessedPath);

            if (!Directory.Exists(UnsentPath))
                Directory.CreateDirectory(UnsentPath);

            if (!Directory.Exists(ErrorPath))
                Directory.CreateDirectory(ErrorPath);

            // Create a universal list of files
            IList<FileInfo> storedFiles = Directory.EnumerateFiles(ProcessedPath)
                .Union(Directory.EnumerateFiles(ErrorPath))
                .Union(Directory.EnumerateFiles(UnsentPath))
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.CreationTimeUtc)
                .ToList();

            long cumulatedSize = 0;
            foreach (var file in storedFiles)
            {
                cumulatedSize += file.Length;
            }

            // Storage limit is given in megabytes 
            long storageLimitInBytes = storageLimit * 1024;

            if (storageLimitInBytes >  cumulatedSize)
            {
                // We are within limits, no need to do anything this time.
                return;
            }

            // Delete files while we are getting well below of the storage limit.
            // Well below here means the half of the limit.
            while (cumulatedSize > storageLimitInBytes/2)
            {
                // Since the universal list we created is ordered by creation time,
                // let's delete the first file (the oldest).
                FileInfo oldestFile = storedFiles.First();
                storedFiles.Remove(oldestFile);

                try
                {
                    File.Delete(oldestFile.FullName);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to delete file %s while applying storage limit! Error message: %s",
                        oldestFile.FullName, e.Message);
                }

                // Decrease the cumulated size, even if the deletion was not successful,
                // otherwise we could end up in an infinite loop.
                cumulatedSize -= oldestFile.Length;
            }
        }

        private static bool ShouldTryResendingData()
        {
            return new Random().NextDouble() < UNSENT_FILES_RESEND_CHANCE;
        }

        private static IList<string> GetPendingTables(string from)
        {
            return Directory.EnumerateFiles(from)
                .Select(f => new FileInfo(f).Name)
                .Where(fileName => fileName.Contains('-'))
                .Select(fileName => fileName.Split('-')[0])
                .Where(tableName => tableName.Length > 0)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Implementation of sending all data files from a directory
        /// </summary>
        /// <param name="output"></param>
        /// <param name="dataPath"></param>
        private static void DoUpload(IOutput output, string dataPath)
        {
            if (!Monitor.TryEnter(DBWriteLock, waitLockTimeout))
            {
                Log.Debug("Skipping DB write as it is already in progress.");
                return;
            }

            try
            {
                // The old code (a while loop) gets stuck if we use the 'unsent' folder
                // as a source.
                var tableNames = GetPendingTables(dataPath);
                MoveAllFiles(OutputWriteResult.Aggregate( tableNames, (table) => {
                    return output.Write(GetFilesOfTable(dataPath, table));
                }));
            }
            catch (DirectoryNotFoundException)
            {
                // This means that the data folder does not exist, which also means that
                // there are no data files to process.
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to write data files to database! Exception message: {0}", e);
            }
            finally
            {
                Monitor.Exit(DBWriteLock);
            }
        }

        /// <summary>
        /// Moves all files of an output write to their proper locations
        /// </summary>
        /// <param name="processedFiles"></param>
        private static void MoveAllFiles(OutputWriteResult processedFiles)
        {
            // Move files to processed folder
            MoveToFolder(processedFiles.successfullyWrittenFiles, ProcessedPath);
            // Move files with errors to the errors folder
            MoveToFolder(processedFiles.failedFiles, ErrorPath);
            // Move files with errors to the errors folder
            MoveToFolder(processedFiles.unsentFiles, UnsentPath);
        }

        /// <summary>
        /// Gets all data files from a dataPath with the prefix specified by table.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static IList<string> GetFilesOfTable(string dataPath, string table)
        {
            // Remove those files that are still being written.
            return Directory.GetFiles(dataPath, table + "-" + DataFilePattern)
                            .Where(fileName => !fileName.Contains(OutputSerializer.IN_PROGRESS_FILE_POSTFIX))
                            .ToList();
        }

        #region Deprecated, but used in tests

        /// <summary>
        /// DbWriterTests uses this function with this signature.
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetFilesOfSameTable()
        {
            return GetFilesOfSameTable(OutputSerializer.DATA_FOLDER);
        }

        /// <summary>
        /// Gives back a list of files for the same table, empty list otherwise 
        /// </summary>
        /// <param name="dataPath">The directory of data files</param>
        /// <returns></returns>
        public static IList<string> GetFilesOfSameTable(string dataPath)
        {
            var allFiles = Directory.GetFiles(dataPath, DataFilePattern);
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
            return Directory.GetFiles(dataPath, pattern + "-" + DataFilePattern)
                            .Where(fileName => !fileName.Contains(OutputSerializer.IN_PROGRESS_FILE_POSTFIX))
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
                    Log.Info("Moved file: {0} to {1}", fileName, outputFolder);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception while moving file {0} to {1}: {2}", fullFileName, outputFolder, ex);
                }
            }
        }

        internal static void MoveToProcessed(IList<string> testFileList)
        {
            MoveToFolder(testFileList, ProcessedPath);
        }

    }
}
