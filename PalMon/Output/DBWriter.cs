﻿using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    // CSV Folder:
    // serverlog-2016-01-28-15-06-00.csv
    // serverlog-2016-01-28-15-06-30.csv
    // threadinfo-2016-01-28-15-06-00.csv


    class DBWriter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string csvPath = @"csv/";
        private const string processedPath = @"csv/processed/";
        private const string csvPattern = @"*.csv";

        public static void Start()
        {
            IList<string> fileList;
            while ((fileList = GetFilesOfSameTable()).Count > 0)
            {
                // BULK COPY
                CopyToDB(fileList);
                // Move files to processed folder
                MoveToProcessed(fileList);
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
            return Directory.GetFiles(csvPath, pattern + "-" + csvPattern);
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
            if (tokens.Length == 0)
            {
                return "";
            }
            return tokens[0];
        }

        public static void CopyToDB(ICollection<string> fileList)
        {
            foreach (var fileName in fileList)
            {
                Log.Info("File to copy: {0}", fileName);
                CachingOutput.Write(fileName);
            }
        }

        public static void MoveToProcessed(IList<string> fileList)
        {
            foreach (var fullFileName in fileList)
            {
                try
                {
                    var fileName = GetFileName(fullFileName);
                    Log.Info("Trying to move: {0}", fileName);
                    var targetFile = processedPath + fileName;
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                    File.Move(fullFileName, targetFile);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception while moving file: {0}", ex);
                }
            }
        }

    }
}
