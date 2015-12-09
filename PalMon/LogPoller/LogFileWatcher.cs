using log4net;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        string watchedFolderPath;
        string filter;
        static Dictionary<string, long> stateOfFiles; //contains filename and actual number of lines in the file
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
            foreach (string fileName in getFileList())
            {
                try
                {
                    pollChangesTo(fileName, changeDelegate);
                }
                catch (Exception e)
                {
                    Log.Error("Unexpected exception occured during reading the log files: " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Read a specific file, and insert the new lines to the database. 
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        static void pollChangesTo(string fullPath, ChangeDelegate changeDelegate)
        {
            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                var offsetInFile = stateOfFiles.ContainsKey(fullPath) ? stateOfFiles[fullPath] : 0;
                sr.BaseStream.Seek(offsetInFile, SeekOrigin.Begin);

                var lines = new List<string>();

                //read the new lines which were appended to the file
                string line;
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);

                // Update the offset
                stateOfFiles[fullPath] = fs.Position;

                // Callback if we have changes
                if (lines.Count > 0)
                    changeDelegate(Path.GetFileName(fullPath), lines.ToArray());

            }
        }
    }
}
