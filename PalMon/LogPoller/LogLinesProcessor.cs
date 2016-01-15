using log4net;
using Newtonsoft.Json;
using PalMon.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PalMon.LogPoller
{
    /// <summary>
    /// Output struct for the LogLinesProcessor
    /// </summary>
    struct LogProcessResult
    {
        public IEnumerable<ServerLogRow> ServerLogs;
        public IEnumerable<FilterStateChangeRow> FilterChanges;
    }

    class LogLinesProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The DNS hostname of the current machine
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Regex for regular filter changes
        /// </summary>
        private const string GROUP_FILTER_RX = @"<groupfilter function='member' level='(.*?)' member='(.*?)'.*?/>";

        /// <summary>
        /// Regex for ALL filters
        /// </summary>
        private const string ALL_FILTER_RX = @"<groupfilter function='level-members' level='([^']*?)' user:ui-enumeration='all'.*?/>";
        private const bool FLUSH_ONLY_ON_DATA = true;

        /// <summary>
        /// Creates a new instance of LogsToDbConverter.
        /// </summary>
        /// <param name="writer">The DataTable Writer to use.</param>
        public LogLinesProcessor()
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
        public void processServerLogLines(CachingOutput output, object writeLock, String filename, String[] jsonStringLines)
        {

            try
            {
                // parse the results
                var parseResult = parseServerLogs(filename, jsonStringLines);
                //  display some status
                var filterStateCount = parseResult.FilterChanges.Count();
                var serverLogsTableCount = parseResult.ServerLogs.Count();

                var statusLine = String.Format("{0} filter {1} and {2} server log {3}",
                    filterStateCount, "row".Pluralize(filterStateCount),
                     serverLogsTableCount, "row".Pluralize(serverLogsTableCount));


                Log.Info("Inserting " + statusLine);


                // If we dont use output.Write
                if (FLUSH_ONLY_ON_DATA)
                {
                    if (filterStateCount > 0)
                    {
                        lock (writeLock)
                        {
                            output.Write(parseResult.FilterChanges.ToArray());
                            //writer.Write(filterStateTable);
                        }
                    }

                    if (serverLogsTableCount > 0)
                    {
                        lock (writeLock)
                        {
                            output.Write(parseResult.ServerLogs.ToArray());
                            //writer.Write(serverLogsTable);
                        }
                    }

                    // Make sure we trigger a flush after the data is written
                    // so the output can trigger writing to the database.
                    output.FlushIfNeeded();
                }
                // If we want to make sure that the results are flushed
                // on polls even if there is no data, simply acquire the write lock
                // and write the data
                else
                {
                    lock (writeLock)
                    {
                        output.Write(parseResult.FilterChanges.ToArray());
                        output.Write(parseResult.ServerLogs.ToArray());
                    }
                }

                Log.Info("Inserted " + statusLine);
            }
            catch (Exception e)
            {
                Log.Fatal("Error while adding to server logs:", e);
                throw;
            }


        }

        private LogProcessResult parseServerLogs(string filename, string[] jsonStringLines)
        {
            // create lists to hold our output
            var serverLogs = new List<ServerLogRow>();
            var filterChanges = new List<FilterStateChangeRow>();

            Log.Info("Trying to parse " + jsonStringLines.Length + " rows of new log data.");
            foreach (var jsonString in jsonStringLines)
            {
                dynamic jsonraw = ParseLogRowJson(jsonString);

                // If we failed to parse then skip this line
                if (jsonraw == null) continue;


                //if we find "eqc-log-cache-key" key then we insert into filter_state_audit table
                if ((jsonraw.k == "eqc-log-cache-key") || (jsonraw.k == "qp-batch-summary"))
                {
                    // try to fetch the cache key value

                    string cacheKeyValue = jsonraw.v["cache-key"];
                    if (cacheKeyValue == null) cacheKeyValue = jsonraw.v.ToString();

                    // Skip if cache key is null
                    if (cacheKeyValue == null)
                    {
                        Log.Error("Cache-key input value was null!");
                    }
                    else
                    {
                        insertToFilterState(cacheKeyValue, filterChanges, jsonraw);
                        insertAllFilters(cacheKeyValue, filterChanges, jsonraw);
                    }

                }

                // Finally insert into the server logs table
                insertIntoServerLogsTable(filename, serverLogs, jsonraw);
            }

            return new LogProcessResult
            {
                FilterChanges = filterChanges,
                ServerLogs = serverLogs
            };

        }

        /// <summary>
        /// Try to parse a single row of JSON logs
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private static dynamic ParseLogRowJson(string jsonString)
        {
            // Parse the json
            try
            {
                return JsonConvert.DeserializeObject(jsonString);
            }
            catch (Exception e)
            {
                Log.Error("Json parse exception occured in string: '" + jsonString + "'", e);
                return null;
            }

        }

        private void insertIntoServerLogsTable(string filename, IList<ServerLogRow> serverLogsTable, dynamic jsonraw)
        {
            string tid = jsonraw.tid;
            serverLogsTable.Add(new ServerLogRow
            {
                TimeStamp = parseJsonTimestamp(jsonraw.ts),

                FileName = filename,
                HostName = HostName,

                ProcessId = (int)jsonraw.pid,

                ThreadId = Convert.ToInt32(tid, 16),
                Severty = jsonraw.sev,
                RequestId = jsonraw.req,
                VizqlSessionId = jsonraw.sess,
                Site = jsonraw.site,
                Username = jsonraw.user,
                Key = jsonraw.k.ToString(),
                Value = jsonraw.v.ToString(),
            });

        }

        /// <summary>
        /// This function is insert some row into the filter_state_audit table in PalMon DB
        /// the sql script: INSERT INTO filter_state_audit VALUES  (@ts, @pid, @tid, @req,@sess, @site,
        ///  @username, @filter_name, @filter_vals, @workbook, @view)       
        /// 
        /// </summary>
        /// <param name="jsonraw">The deserialized JSON object.</param>
        void insertToFilterState(string cache_key_Value, IList<FilterStateChangeRow> filterStateTable, dynamic jsonraw)
        {
            var mc = Regex.Matches(cache_key_Value, GROUP_FILTER_RX);
            foreach (Match m in mc)
            {

                var filterName = m.Groups[1].ToString();
                var filterVals = m.Groups[2].ToString().Replace("&quot;", "");

                string tid = jsonraw.tid;

                filterStateTable.Add(new FilterStateChangeRow
                {
                    TimeStamp = parseJsonTimestamp(jsonraw.ts),
                    ProcessId = (int)jsonraw.pid,
                    ThreadId = Convert.ToInt32(tid, 16),
                    RequestId = jsonraw.req,
                    VizqlSessionId = jsonraw.sess,
                    Site = jsonraw.site,
                    UserName = jsonraw.user,
                    FilterName = filterName,
                    FilterVals = filterVals,
                    HostName = HostName,
                    Workbook = "<WORKBOOK>",
                    View = "<VIEW>",
                    UserIp = "0.0.0.0"
                });
            }

        }

        void insertAllFilters(string cache_key_Value, IList<FilterStateChangeRow> filterStateTable, dynamic jsonraw)
        {

            //insert all filters
            MatchCollection mc2 = Regex.Matches(cache_key_Value, ALL_FILTER_RX);
            foreach (Match m in mc2)
            {
                var filterName = m.Groups[1].ToString();
                //if we find calculation we throw it out (dont insert to DB)
                if (filterName.Contains("Calculation_")) continue;
                string tid = jsonraw.tid;

                filterStateTable.Add(new FilterStateChangeRow
                {
                    TimeStamp = parseJsonTimestamp(jsonraw.ts),
                    ProcessId = (int)jsonraw.pid,
                    ThreadId = Convert.ToInt32(tid, 16),
                    RequestId = jsonraw.req,
                    VizqlSessionId = jsonraw.sess,
                    Site = jsonraw.site,
                    UserName = jsonraw.user,
                    FilterName = filterName,
                    FilterVals = "all",
                    HostName = HostName,
                    Workbook = "<WORKBOOK>",
                    View = "<VIEW>",
                    UserIp = "0.0.0.0"
                });
            }
        }

        //private static ViewPath MakeEmptyViewPath()
        //{
        //    ViewPath viewPath;
        //    viewPath.workbook = "<WORKBOOK>";
        //    viewPath.view = "<VIEW>";
        //    viewPath.ip = "0.0.0.0";
        //    return viewPath;
        //}

        /// <summary>
        /// Helper to update the view's workbook and user ip address from a wizql session
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="jsonraw"></param>
        /// <param name="row"></param>
        //private static void UpdateViewPath(ITableauRepoConn repo, dynamic jsonraw, DataRow row)
        //{

        //    var viewPath = MakeEmptyViewPath();
        //    row["workbook"] = viewPath.workbook;
        //    row["view"] = viewPath.view;
        //    row["user_ip"] = viewPath.ip;
        //}

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
