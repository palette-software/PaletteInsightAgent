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
        private IDbQueries queries;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// We only care about looking up view paths older then this.
        /// </summary>
        private const int DELAY_IN_MINUTES = 1;

        /// <summary>
        /// The max age of an entry to look up.
        /// </summary>
        private const int MAX_AGE_IN_HOURS = 48;

        public ViewPathUpdater(string connectionString, IDbHelper dbHelper, IDbQueries queries)
        {
            this.connectionString = connectionString;
            this.dbHelper = dbHelper;
            this.queries = queries;
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

            SqlConnection.WithConnection(dbHelper, connectionString, (conn) => {
                var updatedCount = 0;
                // Skip if no updates needed.
                while (HasViewpathsToUpdate(conn, queries))
                {
                    var updateList = MakeUpdateList(conn, queries);
                    updatedCount += UpdateViewPaths(conn, queries, repo, updatedCount, updateList);
                }
                // Log some info about this batch
                Log.Info(String.Format("Updated view paths for {0} rows", updatedCount));
            });
        }

        #region Private

        /// <summary>
        /// Helper that returns the number of view paths to update
        /// </summary>
        /// <param name="conn">The connection to use.</param>
        /// <returns></returns>
        private static bool HasViewpathsToUpdate(SqlConnection sql, IDbQueries queries)
        {
            // we need this local var because we are using delegates in the query
            long count = 0;
            sql.query(queries.HAS_FSA_TO_UPDATE_SQL)
                .Params(MostRecentTimestamps())
                .OnScalar(res =>
                {
                    Log.Info(String.Format("Found {0} rows without workbook/view path.", res));
                    count = (long)res;
                })
                .RunScalar();

            return count > 0;
        }

        /// <summary>
        /// Create a list of entries we need to update
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static List<ViewPathLookupEntry> MakeUpdateList(SqlConnection sql, IDbQueries queries)
        {
            var updateList = new List<ViewPathLookupEntry>();

            //// get a batch and update it.
            sql.query(queries.SELECT_FSA_TO_UPDATE_SQL)
                .Params(MostRecentTimestamps())
                .OnResults(res =>
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
                })
                .RunQuery();

            return updateList;
        }

        /// <summary>
        /// Do the actual update of the view paths.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="conn"></param>
        /// <param name="updatedCount"></param>
        /// <param name="updateList"></param>
        /// <returns></returns>
        private static int UpdateViewPaths(SqlConnection sql, IDbQueries queries, ITableauRepoConn repo, int updatedCount, List<ViewPathLookupEntry> updateList)
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
                UpdateViewPathInRow(sql, queries, row.ts, row.id, viewPath.workbook, viewPath.view, viewPath.ip);
            }

            return updatedInThisBatchCount;

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
        private static void UpdateViewPathInRow(SqlConnection sql, IDbQueries queries, DateTime ts, object id, string workbook, string view, string userIp)
        {
            // Do the update after we are sure we can update it with valid data
            sql.query(queries.UPDATE_FSA_SQL)
                .Param("workbook", workbook)
                .Param("view", view)
                .Param("user_ip", userIp)
                .Param("id", id)
                .RunStatement();
        }

        #endregion

        /// <summary>
        /// Returns a dictionnary with "ts" and "min_ts" keys covering the interval
        /// of the timestamps we are interested in.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string,object> MostRecentTimestamps()
        {
            return new Dictionary<string, object>
            {
                {"ts", DateTime.UtcNow.AddMinutes(-1 * DELAY_IN_MINUTES)},
                {"min_ts", DateTime.UtcNow.AddHours(-1 * MAX_AGE_IN_HOURS)}
            };
        }

    }



}
