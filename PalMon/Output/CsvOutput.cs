using System;
using System.IO;
using System.Data;
using NLog;

namespace PalMon.Output
{
    class CsvOutput
    {
        private const string CSV_DATETIME_FORMAT = "yyyy-MM-dd--HH-mm-ss";
        public  const string IN_PROGRESS_FILE_POSTFIX = ".writing";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Write(DataTable table)
        {
            // skip writing if no data
            var rowCount = table.Rows.Count;
            if (rowCount == 0)
            {
                Log.Debug("No data - skipping CSV write.");
                return;
            }

            // write out if any
            var csvFileName = GetCSVFile(table.TableName);

            // First create the file name with a postfix, so that the bulk copy
            // loader won't touch this file, until it is being written.
            var inProgressCsvFileName = csvFileName + IN_PROGRESS_FILE_POSTFIX;

            WriteCsvFile(inProgressCsvFileName, table);

            // remove any rows from the csv queue
            table.Rows.Clear();

            // Remove the postfix to signal that the file write is done.
            File.Move(inProgressCsvFileName, csvFileName);
            Log.Info("{0} {1} written to CSV file: {2}", rowCount, "row".Pluralize(rowCount), csvFileName);
        }

        /// <summary>
        /// Gets the filename of our CSV file and creates 
        /// any directories we may need.
        /// </summary>
        /// <param name="fileBaseName"></param>am>
        /// <returns></returns>
        private static string GetCSVFile(string fileBaseName)
        {
            var dateString = DateTimeOffset.Now.UtcDateTime.ToString(CSV_DATETIME_FORMAT);
            // get a new filename
            var fileName = String.Format("csv/{0}-{1}.csv", fileBaseName, dateString);

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

        /// <summary>
        /// Writes the CSV out. This method appends to the file if it
        /// already exists so we can just add more rows while 
        /// making sure the file is flushed and closed after each write
        /// </summary>
        /// <param name="fileName"></param>
        private static void WriteCsvFile(string fileName, DataTable table)
        {
            var fileExists = File.Exists(fileName);

            using (var streamWriter = File.AppendText(fileName))
            using (var csvWriter = new CsvHelper.CsvWriter(streamWriter))
            {
                // only write the header if the file does not exists
                if (!fileExists)
                {
                    DataTableUtils.WriteCSVHeader(table, csvWriter);
                }

                DataTableUtils.WriteCSVBody(table, csvWriter);
            }
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
