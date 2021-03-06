﻿using CsvHelper;
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
            AddMetadata(table, LogTables.makeServerLogsTable("json"));
            AddMetadata(table, LogTables.makeServerLogsTable("plain"));
            AddMetadata(table, ThreadTables.makeThreadInfoTable());
            AddMetadata(table, CounterSampler.makeCounterSamplesTable());
        }
    }
}
