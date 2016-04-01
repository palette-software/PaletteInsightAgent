﻿using System;
using System.IO;
using System.Data;
using NLog;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace PaletteInsightAgent.Output
{
    class OutputSerializer
    {
        public const string DATA_FOLDER = "data/";
        /// <summary>
        /// Since we may write multiple outputs from the same serverlogs file, the output
        /// may have conflicts if only seconds-precision is used, so we have to use microseconds
        /// which are unlikely to conflict.
        /// </summary>
        private const string FILENAME_DATETIME_FORMAT = "yyyy-MM-dd--HH-mm-ss-ffffff";
        public const string IN_PROGRESS_FILE_POSTFIX = ".writing";
        public const string MAX_ID_PREFIX = "maxid";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static IWriter Writer = new CsvSerializer();
        public static string Extension
        {
            get
            {
                return Writer.Extension;
            }
        }

        public static void Write(DataTable table, string maxId = null)
        {
            // skip writing if no data
            var rowCount = table.Rows.Count;
            if (rowCount == 0)
            {
                Log.Debug("No data - skipping CSV write.");
                return;
            }

            // write out if any
            var dataFileName = GetDataFile(table.TableName);
            try
            {

                Writer.WriteDataFile(dataFileName, table);

                // remove any rows from the csv queue
                table.Rows.Clear();

                // write the maxid file if it is not null
                if (maxId != null)
                {
                    var maxIdFileName = dataFileName + MAX_ID_PREFIX;
                    File.AppendAllText(maxIdFileName, maxId);
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to write {0} table contents to CSV file '{1}'! Exception message: {2}", table.TableName, dataFileName, e.Message);
            }
        }

        public static void Delete(string tableName)
        {
            Directory.EnumerateFiles(DATA_FOLDER, tableName + "*.csv*")
                .Select(f => new FileInfo(f))
                .ToList()
                .ForEach(f => f.Delete());
        }

        /// <summary>
        /// Gets the filename of our CSV file and creates 
        /// any directories we may need.
        /// </summary>
        /// <param name="fileBaseName"></param>am>
        /// <returns></returns>
        private static string GetDataFile(string fileBaseName)
        {
            var dateString = DateTimeOffset.Now.UtcDateTime.ToString(FILENAME_DATETIME_FORMAT);
            // get a new filename
            var fileName = String.Format("{0}{1}-{2}{3}", DATA_FOLDER, fileBaseName, dateString, Writer.Extension);

            // try to create the directory of the output
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            catch (Exception)
            {
                // Do nada
            }

            return fileName;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            // Do nothing
            return;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
