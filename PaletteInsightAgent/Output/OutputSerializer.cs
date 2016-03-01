using System;
using System.IO;
using System.Data;
using NLog;
using System.Globalization;
using System.Collections.Generic;

namespace PaletteInsightAgent.Output
{
    class OutputSerializer
    {
        public const string DATA_FOLDER = "data/";
        private const string FILENAME_DATETIME_FORMAT = "yyyy-MM-dd--HH-mm-ss";
        public  const string IN_PROGRESS_FILE_POSTFIX = ".writing";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static IWriter Writer = new CsvSerializer();
        public static string Extension
        {
            get
            {
                return Writer.Extension;
            }
        }

        public static void Write(DataTable table)
        {
            // skip writing if no data
            var rowCount = table.Rows.Count;
            if (rowCount == 0)
            {
                Log.Debug("No data - skipping CSV write.");
                return;
            }

            try
            {
                // write out if any
                var dataFileName = GetDataFile(table.TableName);

                // First create the file name with a postfix, so that the bulk copy
                // loader won't touch this file, until it is being written.
                var inProgressFileName = dataFileName + IN_PROGRESS_FILE_POSTFIX;

                Writer.WriteDataFile(inProgressFileName, table);

                // remove any rows from the csv queue
                table.Rows.Clear();

                // Remove the postfix to signal that the file write is done.
                File.Move(inProgressFileName, dataFileName);
                Log.Info("{0} {1} written to CSV file: {2}", rowCount, "row".Pluralize(rowCount), dataFileName);
            }
            catch (Exception e)
            {
                Log.Error("Failed to write {0} table contents to CSV file! Exception message: {1}", table.TableName, e.Message);
            }
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
