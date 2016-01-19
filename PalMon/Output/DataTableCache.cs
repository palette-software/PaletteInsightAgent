using CsvHelper;
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
        private const int FlushTimeInSeconds = 10;
        private DataTable queue;

        private string fileBaseName;

        public string TableName { get { return queue.TableName; } }

        /// <summary>
        /// On each flush, this delegate will be called with the CSV file name and the objects
        /// in the queue before the flush.
        /// 
        /// Upon returning, the queue is cleared
        /// </summary>
        Action<string, DataTable> onFlushDelegate;


        public DataTableCache(string baseName, DataTable structureTable, Action<string, DataTable> onFlush)
        {
            fileBaseName = baseName;
            onFlushDelegate = onFlush;

            // create the new datatable
            queue = new DataTable(structureTable.TableName);
            // copy the columns
            foreach (DataColumn c in structureTable.Columns)
            {
                queue.Columns.Add(c.ColumnName, c.DataType);
            }

            lastOutputDate = DateTimeOffset.MinValue;

        }

        public void Put(DataTable rows)
        {
            // validate table name
            if (rows.TableName != queue.TableName)
                throw new ArgumentException(String.Format("Invalid data table given:{0} instead of {1}", rows.TableName, queue.TableName));

            // validate column count
            if (rows.Columns.Count != queue.Columns.Count)
                throw new ArgumentException(String.Format("Invalid data table columns:{0} instead of {1}", rows.Columns.Count, queue.Columns.Count));

            // validate columns
            for (var i = 0; i < rows.Columns.Count; ++i)
            {
                var colIn = rows.Columns[i];
                var colHave = queue.Columns[i];

                if (colIn.ColumnName != colHave.ColumnName || colIn.DataType != colHave.DataType)
                    throw new ArgumentException(String.Format("Mismatching column in datatable: {0} instead of {1}", colIn.ColumnName, colHave.ColumnName));
            }

            Console.Out.WriteLine(String.Format("+ Got {0} rows of {1} - cache is {2} rows - flush at {3}",
                rows.Rows.Count, rows.TableName, queue.Rows.Count, nextOutputDate));

            // start a new output if we need to and flush the existing output
            var newFileStarted = StartNewFileIfNeeded();

            // add all rows to the queue datatable
            foreach (DataRow row in rows.Rows)
            {
                queue.Rows.Add(row.ItemArray);
            }

        }


        /// <summary>
        /// Starts a new file if needed and tries to write the results to the database
        /// </summary>
        public void FlushCacheIfNeeded()
        {
            StartNewFileIfNeeded();
        }

        #region CSV output

        private DateTimeOffset lastOutputDate;
        private DateTimeOffset nextOutputDate;


        /// <summary>
        /// Starts a new CSV file
        /// </summary>
        private void StartNewFile()
        {
            var hasRows = !(queue.Rows.Count == 0);
            // If there is no last output time is set, and the queue is not empty, we have trouble
            if (lastOutputDate == DateTimeOffset.MinValue && hasRows)
            {
                throw new ArgumentNullException("No CSV filename set while the queue is not empty!");
            }

            // Output the existing data to CSV if we need to
            if (lastOutputDate != DateTimeOffset.MinValue && hasRows)
            {
                // get a new filename
                var lastFileName = String.Format("{0}-{1:yyyy-MM-dd--HH-mm-ss}.csv", fileBaseName, lastOutputDate.UtcDateTime);

                // try to create the directory of the output
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(lastFileName));
                }
                catch (Exception e)
                {
                    // Do nada
                }

                WriteCsvFile(lastFileName);

                // Call the flush function with the data in the queue
                onFlushDelegate(lastFileName, queue);

                // after the flush delegate is called and the CSV is written, clear our queue
                queue.Rows.Clear();
            }

            // set up the output timestamps
            lastOutputDate = DateTimeOffset.UtcNow;
            nextOutputDate = lastOutputDate.AddSeconds(FlushTimeInSeconds);
        }

        /// <summary>
        /// Writes the CSV out
        /// </summary>
        /// <param name="lastFileName"></param>
        private void WriteCsvFile(string lastFileName)
        {
            using (var streamWriter = File.CreateText(lastFileName))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                // write the csv header
                foreach (DataColumn column in queue.Columns)
                {
                    csvWriter.WriteField(column.ColumnName);
                }
                csvWriter.NextRecord();


                var columnCount = queue.Columns.Count;

                foreach (DataRow row in queue.Rows)
                {
                    try
                    {
                        for (var i = 0; i < columnCount; i++)
                        {
                            if (row[i] != null)
                            {
                                csvWriter.WriteField(row[i]);
                            }
                            else
                            {
                                csvWriter.WriteField("");
                            }
                        }
                        csvWriter.NextRecord();
                    }
                    catch (CsvWriterException ex)
                    {
                        Console.Error.WriteLine("Error writing record to CSV: " + ex.Message);
                    }
                }
            }
        }


        /// <summary>
        /// Only start a new file if we need to
        /// </summary>
        /// <returns></returns>
        private bool StartNewFileIfNeeded()
        {
            // if either the last or next update date is null or the time is over the next output date
            var needsNewFile = (lastOutputDate == null) || (nextOutputDate == null) || (DateTimeOffset.UtcNow >= nextOutputDate);

            if (!needsNewFile) return false;

            StartNewFile();
            return true;
        }
        #endregion




    }
}
