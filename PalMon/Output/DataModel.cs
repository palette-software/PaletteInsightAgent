using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    /// <summary>
    /// A single row in the filter_state_audit table
    /// </summary>

    [DelimitedRecord(",")]
    public class FilterStateChangeRow
    {
        public DateTime TimeStamp;
        public string RequestId;
        public string VizqlSessionId;
        public int ProcessId;
        public int ThreadId;

        public string UserName;
        public string FilterName;
        public string FilterVals;

        public string Site;
        public string Workbook;
        public string View;

        public string HostName;

        public string UserIp;
    }


    /// <summary>
    /// A single row in the serverlogs table
    /// </summary>
    [DelimitedRecord(",")]
    public class ServerLogRow
    {
        public DateTime TimeStamp;

        public string FileName;
        public string HostName;

        public int ProcessId;
        public int ThreadId;

        public string Severty;

        public string RequestId;
        public string VizqlSessionId;

        public string Site;
        public string Username;
        public string Key;
        public string Value;
    }


    public struct ConvertedRows
    {
        public string TableName;
        public string[] Columns;
        // Keep this IEnumerable so we can be somewhat lazy
        public IEnumerable<object[]> Rows;
    }

    public class DataConverter
    {
        /// <summary>
        /// Helper method to get a list of string from a colon-separated
        /// list of values
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string[] FromString(string s)
        {
            return s.Split(',').Select(x => x.Trim()).ToArray();
        }

        private static readonly string[] filterStateChangeRow =
            FromString("ts, req, sess, pid, tid, username, filter_name, filter_vals, site, workbook, view, hostname, user_ip");

        public static ConvertedRows Convert(FilterStateChangeRow[] r)
        {
            return new ConvertedRows
            {
                TableName = "filter_state_audit",
                Columns = filterStateChangeRow,
                Rows = r.Select(x => new object[] {
                    x.TimeStamp, x.RequestId, x.VizqlSessionId,
                    x.ProcessId, x.ThreadId,
                    x.UserName,
                    x.FilterName, x.FilterVals,
                    x.Site, x.Workbook, x.View,
                    x.HostName, x.UserIp
                }),
            };
        }


        private static readonly string[] serverLogRowColumns =
            FromString("filename, host_name, ts, pid, tid, sev, req, sess, site, username, k,v");

        public static ConvertedRows Convert(ServerLogRow[] r)
        {
            return new ConvertedRows
            {
                TableName = "serverlogs",
                Columns = serverLogRowColumns,
                Rows = r.Select(x => new object[] {
                    x.FileName, x.HostName, x.TimeStamp,
                    x.ProcessId, x.ThreadId, x.Severty,

                    x.RequestId, x.VizqlSessionId,
                    x.Site, x.Username, x.Key, x.Value
                }),
            };
        }

    }


    /// <summary>
    /// A single row in the threadinfo table
    /// </summary>
    public class ThreadInfoRow
    {

        public string HostName;
        public string Instance;

        public int ProcessId;
        public int ThreadId;

        public long CpuTime;
    }
}
