using log4net;
using System.Reflection;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
{
    using ChangeDelegate = Action<string, string[]>;

    class LogFileWatcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        string watchedFolderPath;
        string filter; //e.g "*.txt"
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
            string[] fileEntries = Directory.GetFiles(watchedFolderPath, filter, SearchOption.AllDirectories);
            foreach (string fileName in fileEntries)
            {
                long numberOfLines = 0;
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (sr.ReadLine() != null)
                        numberOfLines++;
                }
                if (!stateOfFiles.ContainsKey(fileName))
                    stateOfFiles.Add(fileName, numberOfLines);
                Console.WriteLine(fileName + ": " + numberOfLines);
            }
        }

        ///// <summary>
        ///// Go over the files in a specific folder, and watch for changes.
        ///// </summary>                  
        //public void watchChanges(ChangeDelegate changeDelegate)
        //{
        //    while (true)
        //    {
        //        Thread.Sleep(1500);
        //        watchChangeCycle(changeDelegate);
        //    }

        //}

        public void watchChangeCycle(ChangeDelegate changeDelegate)
        {
            string[] fileEntries = Directory.GetFiles(watchedFolderPath, filter, SearchOption.AllDirectories);
            foreach (string fileName in fileEntries)
            {
                try
                {
                    writeOutChanges(fileName, changeDelegate);
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
        static void writeOutChanges(string fullPath, ChangeDelegate changeDelegate)
        {
            if (stateOfFiles.ContainsKey(fullPath))
            {
                using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    long offset = 0;

                    //read the first part of the file (no changes here)
                    for (long i = 0; i < stateOfFiles[fullPath]; i++)
                        sr.ReadLine();

                    var lines = new List<string>();
                    //read the new lines which appended to the file
                    while ((line = sr.ReadLine()) != null)
                    {
                        offset++;
                        lines.Add(line);
                        //pg.insertToServerlogsTable(Path.GetFileName(fullPath), line);
                    }
                    // increment the offset
                    stateOfFiles[fullPath] += offset;

                    // Callback if we have changes
                    if (lines.Count > 0)
                        changeDelegate(Path.GetFileName(fullPath), lines.ToArray());

                }
            }
            else
            {
                using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    long lineCounter = 0;
                    while (sr.ReadLine() != null)
                        lineCounter++;
                    stateOfFiles.Add(fullPath, lineCounter);
                }
            }
        }
    }
}
