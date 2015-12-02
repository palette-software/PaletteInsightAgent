using System;
using System.Data;
using log4net;
using System.Reflection;
using Npgsql;
using System.Collections.Generic;
using TabMon.LogPoller.Db;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
{

    public class ViewPathUpdater
    {
        private string connectionString;
        private IDbHelper dbHelper;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// We only care about looking up view paths older then this.
        /// </summary>
        private const int DELAY_IN_MINUTES = 10;

        /// <summary>
        /// The max age of an entry to look up.
        /// </summary>
        private const int MAX_AGE_IN_HOURS = 48;

        public ViewPathUpdater(string connectionString, IDbHelper dbHelper)
        {
            this.connectionString = connectionString;
            this.dbHelper = dbHelper;
        }

        private struct ViewPathLookupEntry
        {
            public long id;
            public string session;
            public DateTime ts;
        }

        public void updateViewPaths(ITableauRepoConn repo)
        {
            // Skip any work if the repo provided is disabled.
            if (repo == null) return;


            // Connect to the DB
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var updatedCount = 0;
                // Skip if no updates needed.
                while (HasViewpathsToUpdate(dbHelper, conn))
                {
                    var updateList = MakeUpdateList(dbHelper, conn);
                    updatedCount += UpdateViewPaths(dbHelper, repo, conn, updatedCount, updateList);
                }
                // Log some info about this batch
                Log.Info(String.Format("Updated view paths for {0} rows", updatedCount));

            }
        }

        /// <summary>
        /// Do the actual update of the view paths.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="conn"></param>
        /// <param name="updatedCount"></param>
        /// <param name="updateList"></param>
        /// <returns></returns>
        private static int UpdateViewPaths(IDbHelper dbHelper, ITableauRepoConn repo, NpgsqlConnection conn, int updatedCount, List<ViewPathLookupEntry> updateList)
        {
            var updatedInThisBatchCount = 0;

            // Update the rows in a separate loop
            foreach (var row in updateList)
            {
                var viewPath = repo.getViewPathForVizQLSessionId(row.session, row.ts);
                if (viewPath.isEmpty())
                {
                    Log.Error(String.Format("==> Cannot find view path for vizQL session='{0}' and timestamp={1}", row.session, row.ts));
                    // update with "<UNKNOWN>"
                    viewPath = ViewPath.Unknown;
                }
                else
                {
                    // increment the count of updated rows
                    updatedInThisBatchCount++;
                }
                // Update the table
                UpdateViewPathInRow(dbHelper, conn, row.ts, row.id, viewPath.workbook, viewPath.view, viewPath.ip);
            }

            return updatedInThisBatchCount;

        }

        /// <summary>
        /// Create a list of entries we need to update
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static List<ViewPathLookupEntry> MakeUpdateList(IDbHelper dbHelper, IDbConnection conn)
        {
            var updateList = new List<ViewPathLookupEntry>();

            // get a batch and update it.
            using (var cmd = dbHelper.MakeSqlCommand(conn, dbHelper.SELECT_FSA_TO_UPDATE_SQL))
            {
                Log.Info("View path update batch start...");

                AddMostRecentTimestampToCommand(dbHelper, cmd);

                using (var res = cmd.ExecuteReader())
                {
                    while (res.Read())
                    {
                        updateList.Add(new ViewPathLookupEntry
                        {
                            id = Convert.ToInt64(res["id"]),
                            session = (string)res["sess"],
                            ts = (DateTime)res["ts"]
                        });
                    }
                }

            }

            return updateList;
        }

        /// <summary>
        /// Updates the view path of a row in the filter_state_audit table.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmd"></param>
        /// <param name="ts"></param>
        /// <param name="id"></param>
        /// <param name="workbook"></param>
        /// <param name="view"></param>
        private static void UpdateViewPathInRow(IDbHelper dbHelper, IDbConnection conn, DateTime ts, object id, string workbook, string view, string userIp)
        {
            // Do the update after we are sure we can update it with valid data
            using (var updateCmd = dbHelper.MakeSqlCommand(conn, dbHelper.UPDATE_FSA_SQL))
            {
                object val = ts;
                dbHelper.AddSqlParameter(updateCmd, "@workbook", workbook);
                dbHelper.AddSqlParameter(updateCmd, "@view", view);
                dbHelper.AddSqlParameter(updateCmd, "@user_ip", userIp);
                dbHelper.AddSqlParameter(updateCmd, "@id", id);

                // run it.
                updateCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Helper that returns the number of view paths to update
        /// </summary>
        /// <param name="conn">The connection to use.</param>
        /// <returns></returns>
        private static bool HasViewpathsToUpdate(IDbHelper dbHelper, IDbConnection conn)
        {
            // Query if we have anything to update
            using (var cmd = dbHelper.MakeSqlCommand(conn, dbHelper.HAS_FSA_TO_UPDATE_SQL))
            {
                AddMostRecentTimestampToCommand(dbHelper, cmd);
                var res = cmd.ExecuteScalar();
                Log.Info(String.Format("Found {0} rows without workbook/view path.", res));
                return ((long)res) > 0;
            }
        }

        /// <summary>
        /// Add an extra @ts parameter to an SQL query with the latest timestamp to look for
        /// </summary>
        /// <param name="cmd"></param>
        private static void AddMostRecentTimestampToCommand(IDbHelper dbHelper, IDbCommand cmd)
        {
            // Only handle events at least 10 minutes old
            // TODO: is the timestamp in UTC or some local time?
            var mostRecentTs = DateTime.UtcNow.AddMinutes(-1 * DELAY_IN_MINUTES);
            dbHelper.AddSqlParameter(cmd, "@ts", mostRecentTs);
            var maxAgeTs = DateTime.UtcNow.AddHours(-1 * MAX_AGE_IN_HOURS);
            dbHelper.AddSqlParameter(cmd, "@min_ts", maxAgeTs);
        }

    }



}
