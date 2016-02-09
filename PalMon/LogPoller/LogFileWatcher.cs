using NLog;
using System.Reflection;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.LogPoller
{
    using ChangeDelegate = Action<string, string[]>;

    class LogFileWatcher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string watchedFolderPath { get; protected set; }
        public string filter { get; protected set; }

        Dictionary<string, long> stateOfFiles; //contains filename and actual number of lines in the file
        public LogFileWatcher(string folderpath, string filter)
        {
            stateOfFiles = new Dictionary<string, long>();
            watchedFolderPath = folderpath;
            this.filter = filter;
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
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                    if (!stateOfFiles.ContainsKey(fileName))
                        stateOfFiles.Add(fileName, fs.Length);

            }
        }

        private string[] getFileList()
        {
            return Directory.GetFiles(watchedFolderPath, filter, SearchOption.AllDirectories);
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
                    var changedThisFile = pollChangesTo(fileName, changeDelegate);
                    hasChanges = changedThisFile || hasChanges;
                }
                catch (Exception e)
                {
                    Log.Error("Unexpected exception occured during reading the log files: " + e.StackTrace);
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
        bool pollChangesTo(string fullPath, ChangeDelegate changeDelegate)
        {
            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {

                // re-open the file when getting the signature so the stream readers offset
                // wont change because of it.
                // When re-using the streamreader here, all files that we list are always
                // marked as changed.
                var signature = getFileSignature(fullPath);

                // use the signature
                var offsetInFile = stateOfFiles.ContainsKey(signature) ? stateOfFiles[signature] : 0;
                sr.BaseStream.Seek(offsetInFile, SeekOrigin.Begin);

                var lines = new List<string>();

                //read the new lines which were appended to the file
                string line;
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);

                // Update the offset
                stateOfFiles[signature] = fs.Position;

                // Callback if we have changes
                if (lines.Count > 0)
                {
                    changeDelegate(Path.GetFileName(fullPath), lines.ToArray());
                    // mark that we have done our duty
                    return true;
                }

                // signal that we did not do any inserts
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the file signature or a null if the file cannot be opened</returns>
        public static string getFileSignature(string fileName)
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
        static string getFileSignature(string fileName, StreamReader sr)
        {
            // the first line is either the first line or empty
            var firstLine = (sr.Peek() > 0) ? sr.ReadLine() : "";
            // use the built-in hash function for now
            var hashCode = firstLine.GetHashCode();

            return String.Format("{0}-{1}", fileName, hashCode);
        }
    }
}
