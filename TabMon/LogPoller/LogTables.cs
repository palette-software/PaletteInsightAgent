using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
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

            //addColumn(table, "id", "System.Int32", true, true);
            addColumn(table, "filename");
            addColumn(table, "host_name");
            addColumn(table, "ts", "System.DateTime");
            addColumn(table, "pid", "System.Int32");
            addColumn(table, "tid", "System.Int32");
            addColumn(table, "sev");
            addColumn(table, "req");
            addColumn(table, "sess");
            addColumn(table, "site");
            addColumn(table, "username");
            addColumn(table, "k");
            addColumn(table, "v");

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

            //addColumn(table, "id", "System.Int32", true, true);
            addColumn(table, "ts", "System.DateTime");
            addColumn(table, "pid", "System.Int32");
            addColumn(table, "tid", "System.Int32");
            addColumn(table, "req");
            addColumn(table, "sess");
            addColumn(table, "site");
            addColumn(table, "username");

            addColumn(table, "filter_name");
            addColumn(table, "filter_vals");
            addColumn(table, "workbook");
            addColumn(table, "view");
            addColumn(table, "hostname");
            addColumn(table, "user_ip");

            //setPrimaryKey(table, "id");
            return table;
        }

        private static void setPrimaryKey(DataTable table, string pkName="id")
        {
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[pkName];
            table.PrimaryKey = PrimaryKeyColumns;
        }

        private static DataColumn addColumn(DataTable table, string colName, string dataType="System.String", bool unique = false, bool readOnly = false)
        {
            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            DataColumn column = new DataColumn();
            column.DataType = System.Type.GetType(dataType);
            column.ColumnName = colName;
            column.ReadOnly = readOnly;
            column.Unique = unique;
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);
            return column;
        }
    };
}
