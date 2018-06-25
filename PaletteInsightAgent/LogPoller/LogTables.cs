using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaletteInsightAgent.Helpers;
using NLog;

namespace PaletteInsightAgent.LogPoller
{
    class LogTables
    {
        public static readonly string SERVERLOGS_TABLE_NAME = "logs";

        //CREATE TABLE serverlogs
        //(
        //  filename text,
        //  host_name text,
        //  line text
        //)
        public static DataTable makeServerLogsTable(string format)
        {
            var tableName = format + SERVERLOGS_TABLE_NAME;
            var table = new DataTable(tableName);
            
            TableHelper.addColumn(table, "line");

            return table;
        }

        public static bool isServerLogsTable(DataTable table)
        {
            return table.TableName.EndsWith(SERVERLOGS_TABLE_NAME);
        }
    };
}
