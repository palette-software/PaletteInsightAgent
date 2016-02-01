using NLog;
using System.Reflection;
using System;
using System.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net;

using PalMon.Output;

namespace PalMon.LogPoller
{
    class LogsToDbConverter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string HostName { get; set; }

        private const string GROUP_FILTER_RX = @"<groupfilter function='member' level='(.*?)' member='(.*?)'.*?/>";

        /// <summary>
        /// Creates a new instance of LogsToDbConverter.
        /// </summary>
        /// <param name="writer">The DataTable Writer to use.</param>
        public LogsToDbConverter()
        {
            this.HostName = Dns.GetHostName();
        }

        /// <summary>
        /// This function insert a row into the Serverlogs table in PalMon DB
        /// sql script: INSERT INTO Serverlogs VALUES (@filename, @host_name, @ts, @pid, @tid, @sev, @req,
        /// @sess, @site, @username, @k, @v)
        /// 
        /// Get a jsonString like this:
        /// {"ts":"2015-11-04T09:35:44.862","pid":11108,"tid":"ac0","sev":"debug","req":"VjnRcApWDfQAAAyQpz0AAAFE",
        /// "sess":"53F596BD0FA8479CA0860F36F27B2346-0:0","site":"Default","user":"tfoldi","k":"msg",
        ///    "v":"   [Time] Building the tuples took 0.0000 sec."}
        /// parse it, and take it to the dataebase. 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="jsonString"></param>
        public void processServerLogLines(object writeLock, String filename, String[] jsonStringLines)
        {
            // Create the datatable
            var serverLogsTable = LogTables.makeServerLogsTable();
            var filterStateTable = LogTables.makeFilterStateAuditTable();

            try
            {
                addServerLogs(filename, jsonStringLines, serverLogsTable, filterStateTable);

                var filterStateCount = filterStateTable.Rows.Count;
                var serverLogsTableCount = serverLogsTable.Rows.Count;

                var statusLine = String.Format("{0} filter {1} and {2} server log {3}",
                    filterStateCount, "row".Pluralize(filterStateCount),
                     serverLogsTableCount, "row".Pluralize(serverLogsTableCount));


                Log.Info("Sending off " + statusLine);


                if (filterStateCount > 0)
                {
                    lock (writeLock)
                    {
                        CachingOutput.Write(filterStateTable);
                    }
                }

                if (serverLogsTableCount > 0)
                {
                    lock (writeLock)
                    {
                        CachingOutput.Write(serverLogsTable);
                    }
                }

                Log.Info("Sent off {0}", statusLine);


            }
            catch (Exception e)
            {
                Log.Fatal(e, "Error while adding to server logs. {0}", e);
                throw;
            }

        }

        private void addServerLogs(string filename, string[] jsonStringLines, DataTable serverLogsTable, DataTable filterStateTable)
        {
            Log.Info("Trying to parse {0} rows of new log data.", jsonStringLines.Length);
            foreach (var jsonString in jsonStringLines)
            {

                // Parse the json
                dynamic jsonraw = null;
                try
                {
                    jsonraw = JsonConvert.DeserializeObject(jsonString);
                }
                catch (Exception e)
                {
                    Log.Error("Json parse exception occured in string: '{0}'. Exception message: {1}", jsonString, e.Message);
                    // skip this line
                    continue;
                }


                //if we find "eqc-log-cache-key" key then we inseret into filter_state_audit table
                if ((jsonraw.k == "eqc-log-cache-key") || (jsonraw.k == "qp-batch-summary"))
                {
                    // try to fetch the cache key value

                    string cacheKeyValue = jsonraw.v["cache-key"];
                    if (cacheKeyValue == null) cacheKeyValue = jsonraw.v.ToString();

                    // Skip if cache key is null
                    if (cacheKeyValue == null)
                    {
                        Log.Error("Regex input value was null!");
                    }
                    else
                    {
                        insertToFilterState(cacheKeyValue, filterStateTable, jsonraw);
                        insertAllFilters(cacheKeyValue, filterStateTable, jsonraw);
                    }

                }

                // Finally insert into the server logs table
                insertIntoServerLogsTable(filename, serverLogsTable, jsonraw);
            }

        }

        private void insertIntoServerLogsTable(string filename, DataTable serverLogsTable, dynamic jsonraw)
        {
            // Add the new row to the table
            var row = serverLogsTable.NewRow();
            string tid = jsonraw.tid;

            row["filename"] = filename;
            row["host_name"] = HostName;
            row["ts"] = parseJsonTimestamp(jsonraw.ts);
            row["pid"] = (int)jsonraw.pid;

            row["tid"] = Convert.ToInt32(tid, 16);
            row["sev"] = jsonraw.sev;
            row["req"] = jsonraw.req;
            row["sess"] = jsonraw.sess;
            row["site"] = jsonraw.site;
            row["username"] = jsonraw.user;
            row["k"] = jsonraw.k;
            row["v"] = jsonraw.v;

            serverLogsTable.Rows.Add(row);
        }

        /// <summary>
        /// This function is insert some row into the filter_state_audit table in PalMon DB
        /// the sql script: INSERT INTO filter_state_audit VALUES  (@ts, @pid, @tid, @req,@sess, @site,
        ///  @username, @filter_name, @filter_vals, @workbook, @view)       
        /// 
        /// </summary>
        /// <param name="jsonraw">The deserialized JSON object.</param>
        void insertToFilterState(string cache_key_Value, DataTable filterStateTable, dynamic jsonraw)
        {
            var mc = Regex.Matches(cache_key_Value, GROUP_FILTER_RX);
            foreach (Match m in mc)
            {
                var row = filterStateTable.NewRow();

                row["ts"] = parseJsonTimestamp(jsonraw.ts);
                row["pid"] = (int)jsonraw.pid;
                row["tid"] = Convert.ToInt32((string)jsonraw.tid, 16);
                row["req"] = jsonraw.req;
                row["sess"] = jsonraw.sess;
                row["site"] = jsonraw.site;
                row["username"] = jsonraw.user;
                row["filter_name"] = m.Groups[1].ToString();
                row["filter_vals"] = m.Groups[2].ToString().Replace("&quot;", "");
                row["hostname"] = HostName;

                UpdateViewPath(row);
                filterStateTable.Rows.Add(row);
            }

        }

        void insertAllFilters(string cache_key_Value, DataTable filterStateTable, dynamic jsonraw)
        {

            //insert all filters
            string pattern2 = @"<groupfilter function='level-members' level='([^']*?)' user:ui-enumeration='all'.*?/>";
            MatchCollection mc2 = Regex.Matches(cache_key_Value, pattern2);
            foreach (Match m in mc2)
            {
                var level = m.Groups[1].ToString();
                //if we find calculation we throw it out (dont insert to DB)
                if (level.Contains("Calculation_"))
                    continue;

                var row = filterStateTable.NewRow();

                row["ts"] = parseJsonTimestamp(jsonraw.ts);
                row["pid"] = (int)jsonraw.pid;
                row["tid"] = Convert.ToInt32((string)jsonraw.tid, 16);
                row["req"] = jsonraw.req;
                row["sess"] = jsonraw.sess;
                row["site"] = jsonraw.site;
                row["username"] = jsonraw.user;
                row["filter_name"] = level;
                row["filter_vals"] = "all";
                row["hostname"] = HostName;

                UpdateViewPath(row);
                filterStateTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// Helper to update the view's workbook and user ip address from a wizql session
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="jsonraw"></param>
        /// <param name="row"></param>
        private static void UpdateViewPath(DataRow row)
        {
            row["workbook"] = "<WORKBOOK>";
            row["view"] = "<VIEW>";
            row["user_ip"] = "0.0.0.0";
        }

        /// <summary>
        /// Creates a DateTime (UTC time) from a date value retrieved from a JSON.
        /// </summary>
        /// <param name="jsonTimeStamp"></param>
        /// <returns>DateTime (UTC time)</returns>
        private static DateTime parseJsonTimestamp(dynamic jsonTimeStamp)
        {
            // Unfortunately first we need to convert the JSON time stamp (currently of which type
            // is Newtonsoft.Json.Linq.JValue) to DateTime and set the DateTimeKind.
            DateTime dt = (DateTime)jsonTimeStamp;
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
            return dt;
        }
    }
}
