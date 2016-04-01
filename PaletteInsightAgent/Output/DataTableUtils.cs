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
