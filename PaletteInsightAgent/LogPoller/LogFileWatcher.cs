using NLog;
using System.Reflection;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.LogPoller
{
    using System.Security.Cryptography;
    using ChangeDelegate = Action<string, string[], int>;

    class LogFileWatcher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string watchedFolderPath { get; protected set; }
        public string filter { get; protected set; }
        public string logFormat { get; protected set; }

        /// <summary>
        /// The limit of lines per batch (so we dont get OOM for large files)
        /// </summary>
        private int linesPerBatch;

        Dictionary<string, long> stateOfFiles; //contains filename and actual number of lines in the file
        public LogFileWatcher(string folderpath, string filter, int linesPerBatch, string logFormat)
        {
            stateOfFiles = new Dictionary<string, long>();
            watchedFolderPath = folderpath;
            this.linesPerBatch = linesPerBatch;
            this.filter = filter;
            this.logFormat = logFormat;
            initFileState();
        }

        /// <summary>
        /// Read the initial state of files. 
        /// Count how many lines are in a file.
        /// Take the results to a Dictionary<string, long> (filename, line count)
        /// </summary>
        void initFileState()
        {
            foreach (string fileName in getFileList())
            {
                // get the file signature
                var signature = getFileSignature(fileName);
                // check if we have a valid signature
                if (signature == null) continue;
                // check & store the state of the file based on the signature
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (!stateOfFiles.ContainsKey(signature))
                        stateOfFiles.Add(signature, fs.Length);

                }

            }
        }

        private string[] getFileList()
        {
            try
            {
                return Directory.GetFiles(watchedFolderPath, filter, SearchOption.AllDirectories).OrderBy(f => new FileInfo(f).LastWriteTime).ToArray();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get files from directory: {0}. Exception: ", watchedFolderPath);
                return new string[0];
            }
        }

        public void watchChangeCycle(ChangeDelegate changeDelegate)
        {
            watchChangeCycle(changeDelegate, () => { });
        }

        public void watchChangeCycle(ChangeDelegate changeDelegate, Action onNoChange)
        {
            var hasChanges = false;
            foreach (string fileName in getFileList())
            {
                try
                {
                    // check if we have any changes
                    // Two lines by intention. Please do not unite into one line as in that case the order of
                    // the operands would be meaningful and in wrong order would cause pollChangesTo to not be called!!!!
                    var changedThisFile = pollChangesToWithLimit(fileName, changeDelegate, linesPerBatch);
                    hasChanges = changedThisFile || hasChanges;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexpected exception occured during reading the log lines of file: {0}! Exception: ", fileName);
                }
            }

            // Run the on-no-change delegate if nothing has changed
            if (!hasChanges)
            {
                onNoChange();
            }
        }

        /// <summary>
        /// Read a specific file, and insert the new lines to the database. 
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        bool pollChangesToWithLimit(string fullPath, ChangeDelegate changeDelegate, int limitOfLines)
        {
            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {

                // re-open the file when getting the signature so the stream readers offset
                // wont change because of it.
                // When re-using the streamreader here, all files that we list are always
                // marked as changed.
                var signature = getFileSignature(fullPath);

                // if the signature is null here, we signal that we did not do any inserts
                if (signature == null) return false;

                // use the signature
                var offsetInFile = stateOfFiles.ContainsKey(signature) ? stateOfFiles[signature] : 0;
                sr.BaseStream.Seek(offsetInFile, SeekOrigin.Begin);

                // Have we read the file to the end?
                bool isFileOver = false;
                // were there any changes to the file?
                bool hadChanges = false;

                int partCount = 0;

                while(!isFileOver)
                {

                    var lines = new List<string>();

                    // 1, if we read null, the file is over
                    // 2, if we read not null, add it to the liens
                    //    if the lines are over the limit, then return them and say that we may have more


                    while (true)
                    {
                        string line = sr.ReadLine();
                        // if the line is null, the file is over
                        if (line == null)
                        {
                            isFileOver = true;
                            break;
                        }

                        // If its not null, we can add it
                        lines.Add(line);

                        // if we are over the line limit, break it up
                        if (lines.Count >= limitOfLines)
                        {
                            break;
                        }
                    }

                    // Callback if we have changes
                    if (lines.Count > 0)
                    {
                        changeDelegate(fullPath, lines.ToArray(), partCount++);
                        // mark that we have done our duty
                        hadChanges = true;
                    }
                }


                // Update the offset for this file
                stateOfFiles[signature] = fs.Position;

                // signal that we did not do any inserts
                return hadChanges;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the file signature or a null if the file cannot be opened</returns>
        public string getFileSignature(string fileName)
        {
            // if the file does not exitst, return a null
            if (!File.Exists(fileName)) return null;

            // otherwise open the file
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                return getFileSignature(fileName, sr);
            }
        }


        /// <summary>
        /// Helper that encapsulates the signature generation from a file stream.
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="sr">The streamReader to use for signature generation</param>
        /// <returns>the file signature</returns>
        string getFileSignature(string fileName, StreamReader sr)
        {
            // return null if we cannot read anything as a C string
            if (sr.Peek() == 0) return null;

            // return on end of stream
            if (sr.EndOfStream) return null;

            // if we read null, we return null
            var line = sr.ReadLine();
            if (line == null) return null;

            // The signature should consist of the WATCHED_FOLDER|WATCH_MASK|HASH_CODE_OF_FIRST_LINE
            return String.Format("{0}|{1}|{2}", watchedFolderPath, filter, HashOfString(line));
        }

        /// <summary>
        /// The default encoding for the log files (we use this to read the bytes)
        /// </summary>
        private static Encoding defaultStringEncoding = new UTF8Encoding();

        /// <summary>
        /// The hash algorith to use for the signature
        /// </summary>
        private HashAlgorithm signatureHasher = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5"));



        /// <summary>
        /// Hash the given string with the signature hasher
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string HashOfString(string str)
        {
            // if the given string is null, get the hash of an empty string
            var s = (str == null ? "" : str);

            // byte array representation of that string
            var encodedPassword = defaultStringEncoding.GetBytes(s);

            // need MD5 to calculate the hash
            var hash = signatureHasher.ComputeHash(encodedPassword);

            // string representation
            return BitConverter.ToString(hash);
        }
    }
}
