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
        private static readonly CsvConfiguration CsvConfig = new CsvHelper.Configuration.CsvConfiguration {
            CultureInfo = CultureInfo.InvariantCulture,
            Delimiter = "\v",
            QuoteNoFields = true 
        };

        public string Extension { get { return ".csv"; } }

        public void WriteDataFile(string fileName, DataTable table)
        {
            var fileExists = File.Exists(fileName);

            using (var streamWriter = File.AppendText(fileName))
            using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, CsvConfig))
            {
                // only write the header if the file does not exists
                if (!fileExists)
                {
                    WriteCSVHeader(table, csvWriter);
                }

                WriteCSVBody(table, csvWriter);
            }
        }

        /// <summary>
        /// Writes the rows of a datatable ot a csv file
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="csvWriter"></param>
        public static void WriteCSVBody(DataTable queue, CsvHelper.CsvWriter csvWriter)
        {
            var columnCount = queue.Columns.Count;

            foreach (DataRow row in queue.Rows)
            {
                try
                {
                    for (var i = 0; i < columnCount; i++)
                    {
                        if (row[i] != null && !row.IsNull(i))
                        {
                            if (row[i].GetType() == typeof(DateTime))
                            {
                                // In order to have milliseconds instead of only seconds in the string representation
                                // of the timestamp, we need to use a custom ToString() method instead of the
                                // default one. This is a kind-of-ugly workaround.
                                DateTime timestamp = (DateTime)row[i];
                                csvWriter.WriteField(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                            }
                            else if (row[i].GetType() == typeof(string))
                            {
                                csvWriter.WriteField(EscapeForCsv(row[i].ToString()));
                            }
                            else
                            {
                                csvWriter.WriteField(row[i]);
                            }
                        }
                        else
                        {
                            csvWriter.WriteField(@"\N");
                        }
                    }
                    csvWriter.NextRecord();
                }
                catch (CsvWriterException ex)
                {
                    Log.Error(ex, "Error writing record to CSV.");
                }
            }
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