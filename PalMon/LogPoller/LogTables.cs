using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PalMon.Helpers;

namespace PalMon.LogPoller
{
    class LogTables
    {
        //CREATE TABLE serverlogs
        //(
        //  filename text,
        //  host_name text,
        //  ts timestamp without time zone,
        //  pid integer,
        //  tid integer,
        //  sev text,
        //  req text,
        //  sess text,
        //  site text,
        //  username text,
        //  k text,
        //  v text
        //)
        public static DataTable makeServerLogsTable()
        {
            var table = new DataTable("serverlogs");

            //TableHelper.addColumn(table, "id", "System.Int32", true, true);
            TableHelper.addColumn(table, "filename");
            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "ts", "System.DateTime");
            TableHelper.addColumn(table, "pid", "System.Int64");
            TableHelper.addColumn(table, "tid", "System.Int32");
            TableHelper.addColumn(table, "sev");
            TableHelper.addColumn(table, "req");
            TableHelper.addColumn(table, "sess");
            TableHelper.addColumn(table, "site");
            TableHelper.addColumn(table, "username");
            TableHelper.addColumn(table, "k");
            TableHelper.addColumn(table, "v");

            //setPrimaryKey(table, "id");

            return table;
        }



        //CREATE TABLE filter_state_audit
        //(
        //  ts timestamp without time zone,
        //  pid integer,
        //  tid integer,
        //  req text,
        //  sess text,
        //  site text,
        //  username text,
        //  filter_name text,
        //  filter_vals text,
        //  workbook text,
        //  view text
        //)
        public static DataTable makeFilterStateAuditTable()
        {

            var table = new DataTable("filter_state_audit");

            //TableHelper.addColumn(table, "id", "System.Int32", true, true);
            TableHelper.addColumn(table, "ts", "System.DateTime");
            TableHelper.addColumn(table, "pid", "System.Int64");
            TableHelper.addColumn(table, "tid", "System.Int32");
            TableHelper.addColumn(table, "req");
            TableHelper.addColumn(table, "sess");
            TableHelper.addColumn(table, "site");
            TableHelper.addColumn(table, "username");

            TableHelper.addColumn(table, "filter_name");
            TableHelper.addColumn(table, "filter_vals");
            TableHelper.addColumn(table, "workbook");
            TableHelper.addColumn(table, "view");
            TableHelper.addColumn(table, "hostname");
            TableHelper.addColumn(table, "user_ip");

            //setPrimaryKey(table, "id");
            return table;
        }

        private static void setPrimaryKey(DataTable table, string pkName="id")
        {
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[pkName];
            table.PrimaryKey = PrimaryKeyColumns;
        }

    };
}
