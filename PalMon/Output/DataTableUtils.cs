using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    public class DataTableUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Creates a blank datatable from the column information
        /// of another
        /// </summary>
        /// <param name="aTable"></param>
        /// <returns></returns>
        public static DataTable CloneColumns(DataTable aTable)
        {
            // create the new datatable
            var o = new DataTable(aTable.TableName);
            // copy the columns
            foreach (DataColumn c in aTable.Columns)
            {
                o.Columns.Add(c.ColumnName, c.DataType);
            }
            return o;
        }

        /// <summary>
        /// apppends the rows from rows to appendTo
        /// </summary>
        /// <param name="appendTo"></param>
        /// <param name="rows"></param>
        public static void Append(DataTable appendTo, DataTable rows)
        {

            // validate table name
            if (rows.TableName != appendTo.TableName)
                throw new ArgumentException(String.Format("Invalid data table given:{0} instead of {1}", rows.TableName, appendTo.TableName));

            // validate column count
            if (rows.Columns.Count != appendTo.Columns.Count)
                throw new ArgumentException(String.Format("Invalid data table columns:{0} instead of {1}", rows.Columns.Count, appendTo.Columns.Count));

            // validate columns
            for (var i = 0; i < rows.Columns.Count; ++i)
            {
                var colIn = rows.Columns[i];
                var colHave = appendTo.Columns[i];

                if (colIn.ColumnName != colHave.ColumnName || colIn.DataType != colHave.DataType)
                    throw new ArgumentException(String.Format("Mismatching column in datatable: {0} instead of {1}", colIn.ColumnName, colHave.ColumnName));
            }

            // add all rows to the queue datatable
            foreach (DataRow row in rows.Rows)
            {
                appendTo.Rows.Add(row.ItemArray);
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

    }
}
