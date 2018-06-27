using NLog;
using PaletteInsightAgent.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
{
    class FileUploader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static DateTime lastStorageUsageLogTimestamp;

        /// <summary>
        /// The directory we store the succesfully uploaded files
        /// </summary>
        private const string PROCESSED_PREFIX = @"processed\";
        /// <summary>
        /// The directory where the files that have errors (invalid names, etc.)
        /// </summary>
        private const string ERROR_PREFIX = @"errors\";

        public static string DataFilePattern
        {
            get
            {
                return "*" + OutputSerializer.Extension;
            }
        }

        public static string ProcessedPath
        {
            get
            {
                return Path.Combine(OutputSerializer.DATA_FOLDER, PROCESSED_PREFIX);
            }
        }

        public static string ErrorPath
        {
            get
            {
                return Path.Combine(OutputSerializer.DATA_FOLDER, ERROR_PREFIX);
            }
        }

        public static readonly object FileUploadLock = new object();
        public static readonly int fileUploadLockTimeout = 1000;

        /// <summary>
        /// Start a single write loop
        /// </summary>
        /// <param name="output"></param>
        public static void Start(IOutput output, int processedFilesTTL, long storageLimit)
        {
            DoUpload(output, OutputSerializer.DATA_FOLDER);
            DeleteOldFiles(ProcessedPath, processedFilesTTL);
            // Storage limit is given in megabytes in config, but we will work with
            // bytes while applying the storage limit
            ApplyStorageLimit(storageLimit * 1024 * 1024);
        }

        private static void DeleteOldFiles(string from, int ttl)
        {
            if (!Directory.Exists(from))
            {
                // No directory, no old files in there.
                return;
            }

            Directory.EnumerateFiles(from)
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < DateTime.Now.AddSeconds(-ttl))
                .ToList()
                .ForEach(f => f.Delete());
        }

        /// <summary>
        /// Deletes oldest file so that storage size can fit in the configured
        /// value. For example if the storage limit is set to 1Gb and the
        /// size of the stored files are larger than 1Gb, then oldest files
        /// will be started to be deleted, until the size of the stored files
        /// are less than half of the configured limit (which is 500 Mb in this
        /// example).
        /// </summary>
        /// <param name="storageLimitInBytes">Given in bytes</param>
        private static void ApplyStorageLimit(long storageLimitInBytes)
        {
            IList<FileInfo> storedFiles = CollectStoredFiles();

            long cumulatedSize = 0;
            foreach (var file in storedFiles)
            {
                try
                {
                    cumulatedSize += file.Length;
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is FileNotFoundException)
                    {
                        // This is not an error, because file may be gone after we iterated them.
                        // Either it has been uploaded in the meantime, or it has been renamed
                        // because the CSV writing finished. (Removed the .writing postfix.)
                    }
                    else
                    {
                        Log.Error("Unexpected error while determining size of file: {0}. Error message: {1}", file.Name, ex.Message);
                    }
                }
            }

            if (DateTime.UtcNow - lastStorageUsageLogTimestamp > TimeSpan.FromMinutes(10))
            {
                // Last storage usage was logged more than 10 minutes ago, so it's time to log again
                // Only log up to 3 decimal places
                Log.Info("Storage usage of agent data folder: {0} Mb", ((double)cumulatedSize / (1024 * 1024)).ToString("0.###"));
                lastStorageUsageLogTimestamp = DateTime.UtcNow;
            }

            if (storageLimitInBytes > cumulatedSize)
            {
                // We are within limits, no need to do anything this time.
                return;
            }

            // Delete files while we are getting well below of the storage limit.
            // Well below here means the half of the limit.
            foreach (var file in storedFiles)
            {
                try
                {
                    // Directory separators are not included in the directory name
                    if (!file.DirectoryName.EndsWith(PROCESSED_PREFIX.TrimEnd(new char[] { '\\', '/' })))
                    {
                        // Deleting an already processed file is not a big deal, but deleting
                        // other files means dataloss.
                        Log.Warn("Deleting unprocessed file because of storage limit: {0} File creation time: {1}",
                            file.FullName, file.CreationTimeUtc);
                    }
                    File.Delete(file.FullName);

                    cumulatedSize -= file.Length;
                    if (cumulatedSize <= storageLimitInBytes / 2)
                    {
                        // We have deleted enough files to get well below the storage limit.
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to delete file {0} while applying storage limit! Exception: ",
                        file.FullName);
                }                
            }
        }

        /// <summary>
        /// Returns an ordered list of files found in "processed", "error"
        /// and "unsent" folders. The first item of the resulting list
        /// is the oldest file.
        /// </summary>
        private static IList<FileInfo> CollectStoredFiles()
        {
            // Collect all the stored files (data, data/processed, data/error) into an ordered
            // list, where the first item is going to be the oldest file.
            try
            {
                return Directory.EnumerateFiles(OutputSerializer.DATA_FOLDER, "*.*", SearchOption.AllDirectories)
                    .Select(file => new FileInfo(file))
                    .OrderBy(file => file.CreationTimeUtc)
                    .ToList();
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Log.Warn("Data directory not found while collecting stored files! Error message: {0}", dnfe.Message);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to collect stored files! Exception: {0}", e);
            }

            // Return empty list on collection failure
            return new List<FileInfo>();
        }

        private static IList<string> GetPendingTables(string from)
        {
            try
            {
                return Directory.EnumerateFiles(from)
                    .Union(Directory.EnumerateFiles(from + @"\serverlogs"))
                    .Select(f => new FileInfo(f).Name)
                    .Where(fileName => fileName.Contains('-'))
                    .Select(fileName => fileName.Split('-')[0])
                    .Where(tableName => tableName.Length > 0)
                    .Distinct()
                    .OrderBy(file => file)
                    .ToList();
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Log.Warn("Directory: {0} not found while getting pending tables! Error message: {1}", from, dnfe.Message);
            }
            catch (Exception e)
            {
                Log.Error("Failed to get pending tables from {0}! Exception: {1}", from , e);
            }

            // Return empty list on error
            return new List<string>();
        }

        /// <summary>
        /// Implementation of sending all data files from a directory
        /// </summary>
        /// <param name="output"></param>
        /// <param name="dataPath"></param>
        private static void DoUpload(IOutput output, string dataPath)
        {
            // For all tables we want to upload the csv files for that data table and move the csv after the upload
            // to the appropriate folder. (Processed on success Errors on failure)
            var tableNames = GetPendingTables(dataPath);
            foreach (var table in tableNames)
            {
                try
                {
                    foreach (var csvFile in GetFilesOfTable(dataPath, table))
                    {
                        try
                        {
                            bool isStreaming = IsStreamingTable(csvFile);
                            output.Write(csvFile);
                            // After successful upload, move the file to the processed folder
                            // NOTE: If the file really got uploaded, and there is a network outage at
                            // this point, the streaming table csv might be deleted before moving to 'processed'
                            // folder. In this case we are going to duplicate some records, but its chance is
                            // really-really low. We need to come up with a 100% solution in the future.
                            MoveToFolder(csvFile, ProcessedPath, !isStreaming);

                            // Also move the maxid file to processed, if there is one.
                            if (isStreaming)
                            {
                                // Pending maxid files might be deleted on another thread while we are uploading
                                // files, so if the maxid file disappears during the move, it is not
                                // necessarily a problem.
                                MoveToFolder(MaxIdFileName(csvFile), ProcessedPath, false);
                            }
                        }
                        catch (AggregateException ae)
                        {
                            ae.Handle((x) =>
                            {
                                if (x is HttpRequestException || x is TaskCanceledException)
                                {
                                    // HttpRequestException is expected on network errors. TaskCanceledException is thrown if the async task (HTTP request) timed out.
                                    throw new TemporaryException(String.Format("Unable to upload file: {0} Message: {1}", csvFile, x.Message));
                                }
                                else if (x is IOException)
                                {
                                    if (x.Message.Contains("The process cannot access the file"))
                                    {
                                        // Don't do anything. It means we have concurred with the unfinished File.Move.
                                        // This file will be successfully uploaded in next iteration.
                                        throw new TemporaryException(String.Format("File is still being moved by other thread: {0} Message: {1}", csvFile, x.Message));
                                    }
                                }
                                else if (x is TemporaryException || x is InsightUnauthorizedException)
                                {
                                    // It is already an exception we know how to handle, just pass it on to the handler.
                                    throw x;
                                }

                                Log.Error(x, "Error while uploading file: {0}! Moving to errors folder! Exception: ", csvFile);
                                MoveToFolder(csvFile, ErrorPath);
                                return true;
                            });
                        }
                        catch (FileNotFoundException fne)
                        {
                            Log.Error(fne, "File {0} not found when trying to upload! Exception: ", csvFile);
                        }
                        catch (Exception e)
                        {
                            if (e is IOException && e.Message.Contains("The process cannot access the file"))
                            {
                                Log.Warn("Temporarily could not access file {0} while trying to upload! Exception: {1}", csvFile, e);
                                return;
                            }
                            Log.Error(e, "Failed to upload data file {0}! Moving to errors folder! Exception: ", csvFile);
                            MoveToFolder(csvFile, ErrorPath);
                        }
                    }
                }
                catch (InsightUnauthorizedException iuae)
                {
                    Log.Error(iuae, "Unauthorized attempt to upload file {0}! Blocking file uploads for 30 minutes! Excpetion: ");
                    Thread.Sleep(new TimeSpan(0, 30, 0));
                }
                catch (TemporaryException e)
                {
                    // Nothing to do here. Leave this filetype as is, we will upload in the next iteration
                    Log.Warn("Temporarily unable to upload files for table {0}. Message: {1}", table, e.Message);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while uploading files for table {0}", table);
                }
            }
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
                            .Union(Directory.GetFiles(dataPath + "serverlogs", table + DataFilePattern))                            
                            .Where(fileName => !fileName.Contains(OutputSerializer.IN_PROGRESS_FILE_POSTFIX))
                            .OrderBy(fileName => fileName)
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
            var tokens = fullFileName.Split('\\');
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
        public static void MoveToFolder(string fullFileName, string outputFolder, bool errorOnNotFound = true)
        {
            try
            {
                // create the output directory
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                var fileName = GetFileName(fullFileName);
                Log.Debug("Trying to move: {0}", fileName);
                var targetFile = Path.Combine(outputFolder, fileName);

                // This should never happen but let's just make sure.
                if (Path.GetFullPath(targetFile) == Path.GetFullPath(fullFileName))
                {
                    Log.Error("Skipping moving a file to itself: {0}", fullFileName);
                    return;
                }

                // Delete the output if it already exists. Although it shouldn't exist.
                if (File.Exists(targetFile))
                {
                    Log.Error("Deleting already existing file while moving a new file on it: {0} NewFile: {1}", targetFile, fullFileName);
                    File.Delete(targetFile);
                }

                // Do the actual move
                try
                {
                    File.Move(fullFileName, targetFile);
                }
                catch (FileNotFoundException fne)
                {
                    string logMessage = String.Format("File {0} not found during moving to {1}! Exception: {2}", fullFileName, targetFile, fne);
                    if (errorOnNotFound)
                    {
                        Log.Error(logMessage);
                    }
                    else
                    {
                        Log.Warn(logMessage);
                    }
                    return;
                }
                Log.Info("Moved file: {0} to {1}", fileName, outputFolder);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while moving file {0} to {1}: {2}", fullFileName, outputFolder, ex);
            }
        }

        public static string GetFileNameWithoutPart(string fileName)
        {
            var pattern = new Regex("(.*-[0-9]{4}-[0-9]{2}-[0-9]{2}--[0-9]{2}-[0-9]{2}-[0-9]{2})(.*?)([.].+)$");
            return pattern.Replace(fileName, "$1$3");
        }

        public static string MaxIdFileName(string fileName)
        {
            return GetFileNameWithoutPart(fileName) + "maxid";
        }

        public static bool IsStreamingTable(string fileName)
        {
            return File.Exists(MaxIdFileName(fileName));
        }
    }
}
