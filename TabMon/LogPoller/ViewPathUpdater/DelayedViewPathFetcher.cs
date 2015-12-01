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

        private const string SELECT_FSA_TO_UPDATE_SQL = @"SELECT id, sess, ts FROM filter_state_audit WHERE workbook= '<WORKBOOK>' AND view='<VIEW>' AND workbook_resolved=FALSE LIMIT 100";
        private const string UPDATE_FSA_SQL = @"UPDATE filter_state_audit SET workbook=@workbook, view=@view, user_ip=@user_ip, workbook_resolved=TRUE WHERE id = @id";
        private const string HAS_FSA_TO_UPDATE_SQL = @"SELECT COUNT(1) FROM filter_state_audit WHERE workbook = '<WORKBOOK>' AND view = '<VIEW>' AND workbook_resolved=FALSE";

        //private const 

        //private const string 

        public PostgresViewPathUpdater(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private struct TmpData
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
                    var updateList = new List<TmpData>();

                    // get a batch and update it.
                    using (var cmd = new NpgsqlCommand())
                    {
                        Log.Info("View path update batch start...");

                        PrepareSqlCommand(conn, cmd, SELECT_FSA_TO_UPDATE_SQL);
                        using (var res = cmd.ExecuteReader())
                        {
                            while (res.Read())
                            {
                                updateList.Add(new TmpData {
                                    id = Convert.ToInt64(res["id"]),
                                    session = (string)res["sess"],
                                    ts = (DateTime)res["ts"]
                                });
                            }
                        }

                    }

                    // Update the rows in a separate loop
                    foreach (var row in updateList)
                    {
                        var viewPath = repo.getViewPathForVizQLSessionId(row.session, row.ts);
                        if (viewPath.isEmpty())
                        {
                            Log.Error(String.Format("==> Cannot find view path for vizQL session='{0}' and timestamp={1}", row.session, row.ts));
                            continue;
                        }
                        // increment the count of updated rows
                        updatedCount++;
                        // Update the table
                        UpdateViewPathInRow(conn, row.ts, row.id, viewPath.workbook, viewPath.view, viewPath.ip);
                    }

                    // Log some info about this batch
                    Log.Info(String.Format("Updated view paths for {0} rows", updatedCount));
                }

            }
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
            using (var updateCmd = new NpgsqlCommand())
            {
                PrepareSqlCommand(conn, updateCmd, UPDATE_FSA_SQL);

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

        /// <summary>
        /// Helper that returns the number of view paths to update
        /// </summary>
        /// <param name="conn">The connection to use.</param>
        /// <returns></returns>
        private static bool HasViewpathsToUpdate(NpgsqlConnection conn)
        {
            // Query if we have anything to update
            using (var cmd = new NpgsqlCommand())
            {
                PrepareSqlCommand(conn, cmd, HAS_FSA_TO_UPDATE_SQL);
                var res = cmd.ExecuteScalar();
                return ((long)res) > 0;
            }
        }

        private static void PrepareSqlCommand(NpgsqlConnection conn, NpgsqlCommand cmd, string cmdText)
        {
            cmd.CommandText = cmdText;
            cmd.Connection = conn;
            cmd.CommandType = System.Data.CommandType.Text;
        }
    }

}
