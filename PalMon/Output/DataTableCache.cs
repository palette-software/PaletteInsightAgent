using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{

    /// <summary>
    /// A csv-caching writer
    /// </summary>
    class DataTableCache
    {
        private const string CSV_DATETIME_FORMAT = "yyyy-MM-dd--HH-mm-ss";
        public  const string IN_PROGRESS_FILE_POSTFIX = ".writing";
        private const int CSV_OUTPUT_BATCH_TIME_SECONDS = 30;
        private const int DB_OUTPUT_BATCH_TIME_SECONDS = 60;
        private string fileBaseName;

        public string TableName { get { return csvQueue.TableName; } }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// On each flush, this delegate will be called with the CSV file name and the objects
        /// in the queue before the flush.
        /// 
        /// Upon returning, the queue is cleared
        /// </summary>
        Action<IList<string>> onFlushDelegate;

        DelayedAction csvWrite;
        //DelayedAction dbFlush;

        DataTable csvQueue;
        DataTable dbQueue;

        public DataTableCache(string baseName, DataTable structureTable, Action<IList<string>> onFlush)
        {
            fileBaseName = baseName;
            onFlushDelegate = onFlush;

            dbQueue = DataTableUtils.CloneColumns(structureTable);
            csvQueue = DataTableUtils.CloneColumns(structureTable);


            // Writing to CSV
            csvWrite = new DelayedAction(TimeSpan.FromSeconds(CSV_OUTPUT_BATCH_TIME_SECONDS), (start, end) =>
            {
                // skip writing if no data
                if (csvQueue.Rows.Count == 0)
                {
                    Log.Debug("No data - skipping CSV write.");
                    return;
                }

                // write out if any
                var csvFileName = GetCSVFile(baseName, start);

                // First create the file name with a prefix, so that the bulk copy
                // loader won't touch this file, until it is being written.
                var inProgressCsvFileName = csvFileName + IN_PROGRESS_FILE_POSTFIX;

                Log.Info("Writing to CSV file: {0}", inProgressCsvFileName);
                WriteCsvFile(inProgressCsvFileName, csvQueue);

                // Remove the prefix to signal that the file write is done.
                File.Move(inProgressCsvFileName, csvFileName);

                // remove any rows from the csv queue
                csvQueue.Rows.Clear();
            });
        }

        public void Put(DataTable rows)
        {
            DataTableUtils.Append(dbQueue, rows);
            DataTableUtils.Append(csvQueue, rows);
        }


        public void Tick()
        {
            csvWrite.Tick();
            // dbFlush.Tick();
        }

        #region CSV output

        /// <summary>
        /// Gets the filename of our CSV file and creates 
        /// any directories we may need.
        /// 
        /// The filename returned will be truncated by the hour
        /// so 1 CSV/hour currently
        /// </summary>
        /// <param name="fileBaseName"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        private static string GetCSVFile(string fileBaseName, DateTimeOffset baseDate)
        {
            var dateString = baseDate.UtcDateTime.ToString(CSV_DATETIME_FORMAT);
            // get a new filename
            var fileName = String.Format("{0}-{1}.csv", fileBaseName, dateString);

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
        /// <param name="lastFileName"></param>
        private static void WriteCsvFile(string lastFileName, DataTable queue)
        {
            var fileExists = File.Exists(lastFileName);

            using (var streamWriter = File.AppendText(lastFileName))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                // only write the header if the file does not exists
                if (!fileExists)
                {
                    DataTableUtils.WriteCSVHeader(queue, csvWriter);
                }

                DataTableUtils.WriteCSVBody(queue, csvWriter);
            }
        }


        #endregion




    }
}
