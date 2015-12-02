using System;
using log4net;
using System.Reflection;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
{

    public interface IViewPathUpdater
    {
        void updateViewPaths(ITableauRepoConn repo);
    }


    public class PostgresViewPathUpdater : IViewPathUpdater
    {
        private string connectionString;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// We only care about looking up view paths older then this.
        /// </summary>
        private const int DELAY_IN_MINUTES = 10;

        /// <summary>
        /// The max age of an entry to look up.
        /// </summary>
        private const int MAX_AGE_IN_HOURS = 48;

        // Our query goes from the oldest to the newest unknown entries
        private const string SELECT_FSA_TO_UPDATE_SQL = @"SELECT id, sess, ts FROM filter_state_audit WHERE workbook= '<WORKBOOK>' AND view='<VIEW>' AND ts < @ts AND ts > @min_ts ORDER BY ts asc LIMIT 100";
        private const string UPDATE_FSA_SQL = @"UPDATE filter_state_audit SET workbook=@workbook, view=@view, user_ip=@user_ip WHERE id = @id";
        private const string HAS_FSA_TO_UPDATE_SQL = @"SELECT COUNT(1) FROM filter_state_audit WHERE workbook = '<WORKBOOK>' AND view = '<VIEW>' AND ts < @ts AND ts > @min_ts";

        public PostgresViewPathUpdater(string connectionString)
        {
            this.connectionString = connectionString;
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
                while (HasViewpathsToUpdate(conn))
                {
                    var updateList = MakeUpdateList(conn);
                    updatedCount += UpdateViewPaths(repo, conn, updatedCount, updateList);
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
        private static int UpdateViewPaths(ITableauRepoConn repo, NpgsqlConnection conn, int updatedCount, List<ViewPathLookupEntry> updateList)
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
                UpdateViewPathInRow(conn, row.ts, row.id, viewPath.workbook, viewPath.view, viewPath.ip);
            }

            return updatedInThisBatchCount;

        }

        /// <summary>
        /// Create a list of entries we need to update
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static List<ViewPathLookupEntry> MakeUpdateList(NpgsqlConnection conn)
        {
            var updateList = new List<ViewPathLookupEntry>();

            // get a batch and update it.
            using (var cmd = MakeSqlCommand(conn, SELECT_FSA_TO_UPDATE_SQL))
            {
                Log.Info("View path update batch start...");

                AddMostRecentTimestampToCommand(cmd);

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
        private static void UpdateViewPathInRow(NpgsqlConnection conn, DateTime ts, object id, string workbook, string view, string userIp)
        {
            // Do the update after we are sure we can update it with valid data
            using (var updateCmd = MakeSqlCommand(conn, UPDATE_FSA_SQL))
            {
                object val = ts;
                AddSqlParameter(updateCmd, "@workbook", workbook);
                AddSqlParameter(updateCmd, "@view", view);
                AddSqlParameter(updateCmd, "@user_ip", userIp);
                AddSqlParameter(updateCmd, "@id", id);

                // run it.
                updateCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Helper that returns the number of view paths to update
        /// </summary>
        /// <param name="conn">The connection to use.</param>
        /// <returns></returns>
        private static bool HasViewpathsToUpdate(NpgsqlConnection conn)
        {
            // Query if we have anything to update
            using (var cmd = MakeSqlCommand(conn, HAS_FSA_TO_UPDATE_SQL))
            {
                AddMostRecentTimestampToCommand(cmd);
                var res = cmd.ExecuteScalar();
                Log.Info(String.Format("Found {0} rows without workbook/view path.", res));
                return ((long)res) > 0;
            }
        }

        /// <summary>
        /// Add an extra @ts parameter to an SQL query with the latest timestamp to look for
        /// </summary>
        /// <param name="cmd"></param>
        private static void AddMostRecentTimestampToCommand(NpgsqlCommand cmd)
        {
            // Only handle events at least 10 minutes old
            // TODO: is the timestamp in UTC or some local time?
            var mostRecentTs = DateTime.UtcNow.AddMinutes(-1 * DELAY_IN_MINUTES);
            AddSqlParameter(cmd, "@ts", mostRecentTs);
            var maxAgeTs = DateTime.UtcNow.AddHours(-1 * MAX_AGE_IN_HOURS);
            AddSqlParameter(cmd, "@min_ts", maxAgeTs);
        }

        /// <summary>
        /// Create a new SQL command from an SQL statement.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        private static NpgsqlCommand MakeSqlCommand(NpgsqlConnection conn, string cmdText)
        {
            NpgsqlCommand cmd = null;
            try
            {
                cmd = new NpgsqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = conn;
                cmd.CommandText = cmdText;
                return cmd;

            }
            catch (Exception e)
            {
                if (cmd != null) cmd.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Add a parameter to an SQL command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        private static void AddSqlParameter(NpgsqlCommand cmd, string name, object val)
        {
            var timestampParam = cmd.CreateParameter();
            timestampParam.ParameterName = name;
            timestampParam.Value = val;
            cmd.Parameters.Add(timestampParam);
        }


    }

}
