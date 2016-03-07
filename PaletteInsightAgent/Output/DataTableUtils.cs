using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
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

        private static void AddColumnInfo(DataTable table, string tableName, string columnName, int attnum, string type = "text")
        {
            DataRow row = table.NewRow();
            row["schemaname"] = "public";
            row["tablename"] = tableName;
            row["columnname"] = columnName;
            row["format_type"] = type;
            row["attnum"] = attnum;
            table.Rows.Add(row);
        }

        public static void AddAgentMetadata(DataTable table)
        {
            AddColumnInfo(table, "serverlogs", "filename", 1);
            AddColumnInfo(table, "serverlogs", "host_name", 2);
            AddColumnInfo(table, "serverlogs", "line", 3);

            AddColumnInfo(table, "threadinfo", "host_name", 1);
            AddColumnInfo(table, "threadinfo", "process", 2);
            AddColumnInfo(table, "threadinfo", "ts", 3, "timestamp without time zone");
            AddColumnInfo(table, "threadinfo", "pid", 4, "bigint");
            AddColumnInfo(table, "threadinfo", "tid", 5, "bigint");
            AddColumnInfo(table, "threadinfo", "cpu_time", 6, "bigint");
            AddColumnInfo(table, "threadinfo", "poll_cycle_ts", 7, "timestamp without time zone");
            AddColumnInfo(table, "threadinfo", "start_ts", 8, "timestamp without time zone");
            AddColumnInfo(table, "threadinfo", "thread_count", 9, "integer");
            AddColumnInfo(table, "threadinfo", "working_set", 10, "bigint");
            AddColumnInfo(table, "threadinfo", "thread_level", 11, "boolean");

            AddColumnInfo(table, "countersamples",  "timestamp", 1, "timestamp without time zone");
            AddColumnInfo(table, "countersamples",  "machine", 2);
            AddColumnInfo(table, "countersamples",  "category", 3);
            AddColumnInfo(table, "countersamples",  "instance", 4);

            AddColumnInfo(table, "countersamples",  "name", 5);
            AddColumnInfo(table, "countersamples",  "value", 6, "double precision");
        }
    }
}
