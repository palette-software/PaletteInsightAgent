using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabMon.Helpers;

namespace TabMon.JMXThreadInfoPoller
{
    class JMXThreadTables
    {
        public static DataTable makeJMXThreadInfoTable()
        {
            var table = new DataTable("jmxthreadinfo");

            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "instance");
            TableHelper.addColumn(table, "ts", "System.DateTime");
            TableHelper.addColumn(table, "tid", "System.Int32");
            TableHelper.addColumn(table, "cpu_time", "System.Int64");
            TableHelper.addColumn(table, "user_time", "System.Int64");
            TableHelper.addColumn(table, "allocated_bytes", "System.Int64");

            return table;
        }

        public static void addToTable(DataTable table, ThreadInfo item)
        {
            var row = table.NewRow();
            row["host_name"] = item.host;
            row["instance"] = item.instance;
            row["ts"] = item.pollTimeStamp;
            row["tid"] = item.threadId;
            row["cpu_time"] = item.cpuTime;
            row["user_time"] = item.userTime;
            row["allocated_bytes"] = item.allocatedBytes;

            table.Rows.Add(row);
        }
    }
}
