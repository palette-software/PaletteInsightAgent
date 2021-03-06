﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaletteInsightAgent.Helpers;

namespace PaletteInsightAgent.ThreadInfoPoller
{
    class ThreadTables
    {
        public static readonly string TABLE_NAME = "threadinfo";

        public static DataTable makeThreadInfoTable()
        {
            var table = new DataTable(TABLE_NAME);

            TableHelper.addColumn(table, "host_name");
            TableHelper.addColumn(table, "process");
            TableHelper.addColumn(table, "ts", "System.DateTime");
            TableHelper.addColumn(table, "pid", "System.Int64");
            TableHelper.addColumn(table, "tid", "System.Int64");
            TableHelper.addColumn(table, "cpu_time", "System.Int64");
            TableHelper.addColumn(table, "poll_cycle_ts", "System.DateTime");
            TableHelper.addColumn(table, "start_ts", "System.DateTime");
            TableHelper.addColumn(table, "thread_count", "System.Int32");
            TableHelper.addColumn(table, "working_set", "System.Int64");
            TableHelper.addColumn(table, "thread_level", "System.Boolean");
            TableHelper.addColumn(table, "read_operation_count", "System.Int64");
            TableHelper.addColumn(table, "write_operation_count", "System.Int64");
            TableHelper.addColumn(table, "other_operation_count", "System.Int64");
            TableHelper.addColumn(table, "read_transfer_count", "System.Int64");
            TableHelper.addColumn(table, "write_transfer_count", "System.Int64");
            TableHelper.addColumn(table, "other_transfer_count", "System.Int64");

            return table;
        }

        public static void addToTable(DataTable table, ThreadInfo item)
        {
            var row = table.NewRow();
            row["host_name"] = item.host;
            row["process"] = item.process;
            row["ts"] = item.pollTimeStamp;
            row["pid"] = item.processId;
            row["tid"] = item.threadId;
            row["cpu_time"] = item.cpuTime;
            row["poll_cycle_ts"] = item.pollCycleTimeStamp;
            row["start_ts"] = item.startTimeStamp;
            row["thread_count"] = item.threadCount;
            row["working_set"] = item.workingSet;
            row["thread_level"] = item.threadLevel;
            row["read_operation_count"] = item.readOperationCount;
            row["write_operation_count"] = item.writeOperationCount;
            row["other_operation_count"] = item.otherOperationCount;
            row["read_transfer_count"] = item.readTransferCount;
            row["write_transfer_count"] = item.writeTransferCount;
            row["other_transfer_count"] = item.otherTransferCount;

            table.Rows.Add(row);
        }
    }
}
