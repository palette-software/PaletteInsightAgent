using CsvHelper;
using CsvHelper.Configuration;
using NLog;
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace PaletteInsightAgent.Output
{
    class CsvSerializer : IWriter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly CsvConfiguration CsvConfig = new CsvHelper.Configuration.CsvConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture,
            Delimiter = "\v",
            QuoteNoFields = true
        };

        /// <summary>
        /// The maximum file size per part in bytes (this is a lower limit, the actual files will be
        /// ~ this size + one line + the quotation and separator overhead).
        /// </summary>
        public static int MaxFileSize = 15 * 1024 * 1024;

        public string Extension { get { return ".csv"; } }

        /// <summary>
        /// Tries to write the passed datatable to a series of files with a (rough) limit on the maximum file size.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="table"></param>
        public void WriteDataFile(string fileName, DataTable table)
        {
            // skip empty tables
            if (table.Rows.Count == 0) return;

            // the index of the last row we have written out
            var lastRow = 0;
            var filePartIdx = 0;

            // The directory where we put the CSV files
            var baseDir = Path.GetDirectoryName(fileName);

            while (true)
            {
                // get the output file path
                var fileNameWithPart = String.Format("{0}--part{1:0000}{2}", Path.GetFileNameWithoutExtension(fileName), filePartIdx, Path.GetExtension(fileName));
                var filePathWithPart = Path.Combine(baseDir, fileNameWithPart);

                // First create the file name with a postfix, so that the bulk copy
                // loader won't touch this file, until it is being written.
                var inProgressFileName = String.Format("{0}{1}", filePathWithPart, OutputSerializer.IN_PROGRESS_FILE_POSTFIX);


                var fileExists = File.Exists(inProgressFileName);
                using (var streamWriter = File.AppendText(inProgressFileName))
                using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, CsvConfig))
                {
                    // only write the header if the file does not exists
                    if (!fileExists)
                    {
                        WriteCSVHeader(table, csvWriter);
                    }

                    // update the last output
                    lastRow = WriteCSVBody(table, csvWriter, lastRow, MaxFileSize);
                }

                // After writing the file, move it to its final destination, and 
                // remove the postfix to signal that the file write is done.
                File.Move(inProgressFileName, filePathWithPart);
                Log.Info("[CSV] written final part '{0}' -- {1}/{2} rows", fileNameWithPart, lastRow, table.Rows.Count);
                // if the last row is -1, the write function has finished the whole table
                if (lastRow == -1) return;

                // increment the part idx
                filePartIdx++;
            }

        }

        /// <summary>
        /// Writes the rows of a datatable ot a csv file
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="csvWriter"></param>
        /// <param name="maxSize">
        ///     The maximum size of the current part in bytes. NOTE: this is a lower bound, and a single line may flow over this limit.
        ///     We have seen log lines of up to 5Mb in length, so using 15 here should almost guarantee us a request size of under 20Mb.
        /// </param>
        /// <returns>The index of the last written row, or -1 if the whole table has been written out</returns>
        public static int WriteCSVBody(DataTable queue, CsvHelper.CsvWriter csvWriter, int startRowIdx = 0, Int64 maxSize = 15 * 1024 * 1024)
        {
            var columnCount = queue.Columns.Count;
            var byteCount = 0;

            // the maximum row index we are willing to touch
            var maxRowIdx = queue.Rows.Count;

            // Do an indexed for loop, so we can return the row index
            for (var rowIdx = startRowIdx; rowIdx < maxRowIdx; rowIdx++)
            {
                // try to write the row out
                try
                {
                    // update the byte count at the end of the row write so that if any exceptions happen, the bytescount
                    // wont contain the bytes of the not written lines
                    byteCount += WriteCsvLine(csvWriter, columnCount, queue.Rows[rowIdx]);
                    // compare the current byte count with the maximum and stop after this line if we are over
                    if (byteCount > maxSize)
                    {
                        return rowIdx;
                    }
                }
                catch (CsvWriterException ex)
                {
                    Log.Error(ex, "Error writing record to CSV.");
                    // if we didnt write this line out then we dont increment the row byte count
                }
            }

            // we return -1 indicating that the whole table has been written
            return -1;
        }

        /// <summary>
        /// Writes a single line of a datatable into a csv file
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="columnCount"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static int WriteCsvLine(CsvWriter csvWriter, int columnCount, DataRow row)
        {
            var rowByteCount = 0;
            for (var i = 0; i < columnCount; i++)
            {
                object fieldValue;
                if (row[i] != null && !row.IsNull(i))
                {
                    if (row[i].GetType() == typeof(DateTime))
                    {
                        // In order to have milliseconds instead of only seconds in the string representation
                        // of the timestamp, we need to use a custom ToString() method instead of the
                        // default one. This is a kind-of-ugly workaround.
                        DateTime timestamp = (DateTime)row[i];
                        fieldValue = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    }
                    else if (row[i].GetType() == typeof(string))
                    {
                        // strings need to escaped for postgres-specific characters
                        fieldValue = EscapeForCsv(row[i].ToString());
                    }
                    else
                    {
                        // otherwise let CsvWriter convert it
                        fieldValue = row[i];
                    }
                }
                else
                {
                    // Nulls are escaped as \N
                    fieldValue = @"\N";
                }
                // update the total byte count
                rowByteCount += fieldValue.ToString().Length;
                // Write the current field
                csvWriter.WriteField(fieldValue);
            }
            // output the record
            csvWriter.NextRecord();
            // return the new byte count
            return rowByteCount;
        }

        /// <summary>
        /// Writes a csv header from a datatable
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="csvWriter"></param>
        public static void WriteCSVHeader(DataTable queue, CsvHelper.CsvWriter csvWriter)
        {
            foreach (DataColumn column in queue.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }
            csvWriter.NextRecord();
        }

        public static string EscapeForCsv(string field)
        {
            return field.Replace("\r", "\\015")
                .Replace("\n", "\\012")
                .Replace("\0", "")
                .Replace("\v", "\\013");
        }
    }
}