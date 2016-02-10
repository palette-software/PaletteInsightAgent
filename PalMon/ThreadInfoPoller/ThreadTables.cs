using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PalMon.Helpers;

namespace PalMon.ThreadInfoPoller
{
    class ThreadTables
    {
        public static readonly string THREADINFO_TABLE_NAME = "threadinfo";

        public static DataTable makeThreadInfoTable()
        {
            var table = new DataTable(THREADINFO_TABLE_NAME);

            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "instance");
            TableHelper.addColumn(table, "ts", "System.DateTime");
            TableHelper.addColumn(table, "pid", "System.Int64");
            TableHelper.addColumn(table, "tid", "System.Int64");
            TableHelper.addColumn(table, "cpu_time", "System.Int64");
            TableHelper.addColumn(table, "poll_cycle_ts", "System.DateTime");
            TableHelper.addColumn(table, "start_ts", "System.DateTime");

            return table;
        }

        public static void addToTable(DataTable table, ThreadInfo item)
        {
            var row = table.NewRow();
            row["host_name"] = item.host;
            row["instance"] = item.instance;
            row["ts"] = item.pollTimeStamp;
            row["pid"] = item.processId;
            row["tid"] = item.threadId;
            row["cpu_time"] = item.cpuTime;
            row["poll_cycle_ts"] = item.pollCycleTimeStamp;
            row["start_ts"] = item.startTimeStamp;

            table.Rows.Add(row);
        }
    }
}
