using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaletteInsightAgent.Helpers;

namespace PaletteInsightAgent.LogPoller
{
    class LogTables
    {
        public static readonly string SERVERLOGS_TABLE_NAME = "serverlogs";

        //CREATE TABLE serverlogs
        //(
        //  filename text,
        //  host_name text,
        //  line text
        //)
        public static DataTable makeServerLogsTable()
        {
            var table = new DataTable(SERVERLOGS_TABLE_NAME);

            TableHelper.addColumn(table, "filename");
            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "line");

            return table;
        }
    };
}
