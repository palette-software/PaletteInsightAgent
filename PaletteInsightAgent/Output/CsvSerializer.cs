using CsvHelper;
using CsvHelper.Configuration;
using NLog;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.LogPoller;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace PaletteInsightAgent.Output
{
    class CsvSerializer : IWriter
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly string StaticExtension = ".csv.gz";
        private static readonly CsvConfiguration CsvConfig = new CsvHelper.Configuration.CsvConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture,
            Delimiter = ",",
            QuoteNoFields = true
        };

        /// <summary>
        /// The maximum file size per part in bytes (this is a lower limit, the actual files will be
        /// ~ this size + one line + the quotation and separator overhead).
        /// </summary>
        public static long MaxFileSize = 50 * 1024 * 1024;

        public string Extension { get { return StaticExtension; } }

        /// <summary>
        /// Tries to write the passed datatable to a series of files with a (rough) limit on the maximum file size.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="table"></param>
        public void WriteDataFile(string fileName, DataTable table, bool isFullTable, bool writeHeader, string originalFileName="")
        {
            // skip empty tables
            if (table.Rows.Count == 0) return;

            // the index of the last row we have written out
            var lastRow = 0;
            var filePartIdx = 0;

            bool isServerLogsTable = LogTables.isServerLogsTable(table.TableName);
            if (isServerLogsTable && originalFileName == "")
            {
                Log.Warn("Missing original filename for serverlogs file: '{0}'. Skipping file write.", fileName);
                return;
            }


            // The directory where we put the CSV files
            var baseDir = Path.GetDirectoryName(fileName);

            while (true)
            {
                var filePathWithPart = FindNextAvailableFilename(fileName, filePartIdx);

                // First create the file name with a postfix, so that the bulk copy
                // loader won't touch this file, until it is being written.
                var inProgressFileName = String.Format("{0}{1}", filePathWithPart, OutputSerializer.IN_PROGRESS_FILE_POSTFIX);
                var fileExists = File.Exists(inProgressFileName);

                using (var fileStream = new FileStream(inProgressFileName, FileMode.Append))
                using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
                using (var streamWriter = new StreamWriter(gzipStream))
                {
                    if (isServerLogsTable)
                    {
                        lastRow = useFileWriter(table, streamWriter, fileExists, filePathWithPart, originalFileName);
                    }
                    else
                    {
                        lastRow = useCsvWriter(table, streamWriter, writeHeader, isFullTable, fileExists, filePathWithPart);
                    }
                }
                // After writing the file, move it to its final destination, and 
                // remove the postfix to signal that the file write is done.
                File.Move(inProgressFileName, filePathWithPart);
                Log.Info("[CSV] written final part '{0}' -- {1}/{2} rows", Path.GetFileName(filePathWithPart), lastRow, table.Rows.Count);
                // if the last row is -1, the write function has finished the whole table
                if (lastRow == -1) return;

                if (isFullTable)
                {
                    Log.Error("Splitting full table CSV file: '{0}'! Full table files are always expected to be sent in one piece",
                        Path.GetFileName(filePathWithPart));
                }

                // increment the part idx
                filePartIdx++;
            }
        }

        private int useCsvWriter(DataTable table, StreamWriter streamWriter, bool writeHeader, bool isFullTable, bool fileExists, string filePathWithPart)
        {
            int lastRow = 0;
            using (var csvWriter = new CsvWriter(streamWriter, CsvConfig))
            {
                // Header should be the first line
                if (writeHeader && !fileExists)
                {
                    WriteCSVHeader(table, csvWriter);
                }

                // files for full table must never be chunked to parts
                long maxSize = isFullTable ? long.MaxValue : MaxFileSize;
                string fileNameForCSV = LogTables.isServerLogsTable(table.TableName) ? null : Path.GetFileName(filePathWithPart);
                lastRow = WriteCSVBody(table, csvWriter, lastRow, maxSize, fileNameForCSV);
            }
            return lastRow;
        }

        private int useFileWriter(DataTable table, StreamWriter streamWriter, bool fileExists,
                                  string filePathWithPart, string originalFileName)
        {
            int lastRow = 0;
            var byteCount = 0;

            streamWriter.WriteLine(originalFileName);
            byteCount += originalFileName.Length;

            // the maximum row index we are willing to touch
            var maxRowIdx = table.Rows.Count;

            for (var rowIdx = lastRow; rowIdx < maxRowIdx; rowIdx++)
            {
                // try to write the row out
                try
                {
                    // update the byte count at the end of the row write so that if any exceptions happen, the bytescount
                    // wont contain the bytes of the not written lines
                    string line = table.Rows[rowIdx][0].ToString();
                    byteCount += line.Length;
                    streamWriter.WriteLine(line);

                    // compare the current byte count with the maximum and stop after this line if we are over
                    // and as we have already written one more file let's just return rowIndex plus 1 instead of rowIndex
                    // otherwise we could end up with an infinite loop when there is a row that is bigger than our max file size
                    if (byteCount > MaxFileSize)
                    {
                        return rowIdx + 1;
                    }
                }
                catch (CsvWriterException ex)
                {
                    Log.Error(ex, "Error writing record to non-csv file.");
                    // if we didnt write this line out then we dont increment the row byte count
                }
            }

            return -1;
        }
        /// <summary>
        /// Tries to find the next available output filename for a CSV file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePartIdx"></param>
        /// <param name="baseDir"></param>
        /// <returns></returns>
        private static string FindNextAvailableFilename(string fileName, int filePartIdx)
        {
            // The index we use if there already is a file with this name in the directory
            var seqIdx = 0;

            while (true)
            {

                // get the output file path
                var fileNameWithPart = String.Format("{0}--seq{1:0000}--part{2:0000}{3}", fileName.TrimEnd(StaticExtension.ToCharArray()), seqIdx, filePartIdx, StaticExtension);

                // If it does not exist, we have the name we want
                if (!File.Exists(fileNameWithPart))
                {
                    // IMPORTANT NOTE: This check only works, if there is only one thread creating and writing these files.
                    // This check is not threadsafe.
                    return fileNameWithPart;
                }

                Log.Debug("Increasing seq-id because file '{0}' already exists", fileNameWithPart);

                // otherwise get the next seq id
                seqIdx++;
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
        public static int WriteCSVBody(DataTable queue, CsvHelper.CsvWriter csvWriter, int startRowIdx, long maxSize, string fileName)
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
                    byteCount += WriteCsvLine(csvWriter, columnCount, queue.Rows[rowIdx], fileName);
                    // compare the current byte count with the maximum and stop after this line if we are over
                    // and as we have already written one more file let's just return rowIndex plus 1 instead of rowIndex
                    // otherwise we could end up with an infinite loop when there is a row that is bigger than our max file size
                    if (byteCount > maxSize)
                    {
                        return rowIdx + 1;
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
        private static int WriteCsvLine(CsvWriter csvWriter, int columnCount, DataRow row, string withFileName = null)
        {
            var rowByteCount = 0;

            if (withFileName != null)
            {
                // update the total byte count
                rowByteCount += withFileName.Length;
                // Write the current field
                csvWriter.WriteField(withFileName);
            }

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
                        fieldValue = GreenplumCsvEscaper.EscapeField(row[i].ToString());
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

    }
}