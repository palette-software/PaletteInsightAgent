﻿using System;
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
        public static DataTable makeThreadInfoTable()
        {
            var table = new DataTable("threadinfo");

            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "instance");
            TableHelper.addColumn(table, "ts", "System.DateTimeOffset");
            TableHelper.addColumn(table, "pid", "System.Int32");
            TableHelper.addColumn(table, "tid", "System.Int32");
            TableHelper.addColumn(table, "cpu_time", "System.Int64");

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

            table.Rows.Add(row);
        }
    }
}
