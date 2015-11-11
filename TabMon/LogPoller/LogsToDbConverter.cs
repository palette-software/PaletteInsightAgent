using log4net;
using System.Reflection;
using System;
using System.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net;

using DataTableWriter.Writers;

namespace TabMon.LogPoller
{
    class LogsToDbConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        /// This function insert a row into the Serverlogs table in TabMon DB
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
        public void processServerLogLines(IDataTableWriter writer, object writeLock, String filename, String[] jsonStringLines)
        {
            // Create the datatable
            var serverLogsTable = LogTables.makeServerLogsTable();
            var filterStateTable = LogTables.makeFilterStateAuditTable();

            try
            {
                addServerLogs(filename, jsonStringLines, serverLogsTable, filterStateTable);

                var filterStateCount = filterStateTable.Rows.Count;
                var serverLogsTableCount = serverLogsTable.Rows.Count;

                Log.Info("Writing "
                    + filterStateCount + " filter " + "row".Pluralize(filterStateCount)
                    + " and "
                    + serverLogsTableCount + " server log " + "row".Pluralize(serverLogsTableCount));

                lock(writeLock)
                {
                    if (filterStateCount > 0) writer.Write(filterStateTable);
                    if (serverLogsTableCount > 0) writer.Write(serverLogsTable);
                }
            }
            catch (Exception e)
            {
                Log.Fatal("Error while adding to server logs:", e);
                throw;
            }


        }

        private void addServerLogs(string filename, string[] jsonStringLines, DataTable serverLogsTable, DataTable filterStateTable)
        {
            Log.Info("Trying to parse " + jsonStringLines.Length + " rows of new log data.");
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
                    Log.Error("Json parse exception occured in string: '" + jsonString + "'", e);
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

                        //Log.Info("Parsed into filter_state table: " + filterStateTable.Rows.ToString());
                    }

                }

                // Finally insert into the server logs table
                insertIntoServerLogsTable(filename, serverLogsTable, jsonraw);
            }

            //Log.Info("Parsed into server logs table: " + serverLogsTable.Rows.ToString());
        }

        private void insertIntoServerLogsTable(string filename, DataTable serverLogsTable, dynamic jsonraw)
        {
            // Add the new row to the table
            var row = serverLogsTable.NewRow();

            //row["id"] = 1;
            row["filename"] = filename;
            row["host_name"] = HostName;
            row["ts"] = jsonraw.ts;
            row["pid"] = (int)jsonraw.pid;

            row["tid"] = Convert.ToInt32((jsonraw.tid as string), 16);
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
        /// This function is insert some row into the filter_state_audit table in TabMon DB
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

                var level = m.Groups[1].ToString();
                var member = m.Groups[2].ToString();
                member = member.Replace("&quot;", "");

                var row = filterStateTable.NewRow();

                row["ts"] = jsonraw.ts;
                row["pid"] = (int)jsonraw.pid;
                row["tid"] = Convert.ToInt32((jsonraw.tid as string), 16);
                row["req"] = jsonraw.req;
                row["sess"] = jsonraw.sess;
                row["site"] = jsonraw.site;
                row["username"] = jsonraw.user;
                row["filter_name"] = level;
                row["filter_vals"] = member;
                row["workbook"] = "";
                row["view"] = "";
                row["hostname"] = HostName;
                filterStateTable.Rows.Add(row);
            }

        }


        void insertAllFilters(string cache_key_Value, DataTable filterStateTable, dynamic jsonraw)
        {

            //insert all filters
            string pattern2 = @"<groupfilter function='level-members' level='(.*?)' user:ui-enumeration='(.*?)'.*?/>";
            MatchCollection mc2 = Regex.Matches(cache_key_Value, pattern2);
            foreach (Match m in mc2)
            {
                var level = m.Groups[1].ToString();
                //if we find calculation we throw it out (dont insert to DB)
                if (level.Contains("Calculation_"))
                    continue;

                var member = m.Groups[2].ToString();
                member = member.Replace("&quot;", "");

                var row = filterStateTable.NewRow();

                //var insert_cmd = new NpgsqlCommand(insertQuery, TabMon_conn);
                row["ts"] = jsonraw.ts;
                row["pid"] = (int)jsonraw.pid;
                row["tid"] = Convert.ToInt32((jsonraw.tid as string), 16);
                row["req"] = jsonraw.req;
                row["sess"] = jsonraw.sess;
                row["site"] = jsonraw.site;
                row["username"] = jsonraw.user;
                row["filter_name"] = level;
                row["filter_vals"] = member;
                row["workbook"] = "";
                row["view"] = "";
                row["hostname"] = HostName;
                filterStateTable.Rows.Add(row);
            }
        }

    }
}
