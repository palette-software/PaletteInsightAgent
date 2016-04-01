using CsvHelper;
using NLog;
using PaletteInsightAgent.LogPoller;
using PaletteInsightAgent.Sampler;
using PaletteInsightAgent.ThreadInfoPoller;
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

        private static string GetDBType(string nativeType)
        {
            switch (nativeType)
            {
                case "System.DateTime":
                    return "timestamp without time zone";
                case "System.Int64":
                    return "bigint";
                case "System.Int32":
                    return "integer";
                case "System.Boolean":
                    return "boolean";
                case "System.Double":
                    return "double precision";
                default:
                    return "text";
            }
        }

        private static void AddMetadata(DataTable result, DataTable table)
        {
            var index = 0;
            foreach (DataColumn column in table.Columns)
            {
                index++;
                AddColumnInfo(result, table.TableName, column.ColumnName, index, GetDBType(column.DataType.ToString()));
            }

        }

        public static void AddAgentMetadata(DataTable table)
        {
            AddMetadata(table, LogTables.makeServerLogsTable());
            AddMetadata(table, ThreadTables.makeThreadInfoTable());
            AddMetadata(table, CounterSampler.makeCounterSamplesTable());
        }
    }
}
