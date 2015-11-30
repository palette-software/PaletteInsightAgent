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

        public PostgresViewPathUpdater(string connectionString)
        {
            this.connectionString = connectionString;
        }


        public void updateViewPaths(ITableauRepoConn repo)
        {
            // Skip any work if the repo provided is disabled.
            if (repo == null) return;

            // Connect to the DB
            using (var conn = new NpgsqlConnection(connectionString))
            {
                // Skip if no updates needed.
                while(HasViewpathsToUpdate(conn))
                {
                    // get a batch and update it.
                    using (var cmd = new NpgsqlCommand())
                    {
                        PrepareSqlCommand(conn, cmd, @"SELECT id, sess, ts FROM filter_state_audit WHERE workbook_name = '<WORKBOOK>' AND view_name = '<VIEW>' LIMIT 100");
                        var res = cmd.ExecuteReader();

                        while(res.Read())
                        {
                            string sess = (string)res["sess"];
                            DateTime ts = (DateTime)res["ts"];
                            var viewPath = repo.getViewPathForVizQLSessionId(sess, ts);

                            // log the fact that we may not be able to find the view path.
                            if (viewPath.isEmpty())
                            {
                                Log.Error(String.Format("==> Cannot find view path for vizQL session='{0}' and timestamp={1}", sess, ts));
                                continue;
                            }

                            // Update the table
                            UpdateViewPathInRow(conn, cmd, ts, res["id"], viewPath.workbook, viewPath.view);
                        }
                    }
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
        private static void UpdateViewPathInRow(NpgsqlConnection conn, NpgsqlCommand cmd, DateTime ts, object id, string workbook, string view)
        {
            // Do the update after we are sure we can update it with valid data
            using (var updateCmd = new NpgsqlCommand())
            {
                PrepareSqlCommand(conn, cmd, @"UPDATE filter_state_audit SET workbook_name= @workbook, view_name = @view WHERE id = @id;");

                object val = ts;
                AddSqlParameter(cmd, "@workbook", workbook);
                AddSqlParameter(cmd, "@view", view);
                AddSqlParameter(cmd, "@id", id);

                // run it.
                cmd.ExecuteNonQuery();
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
                PrepareSqlCommand(conn, cmd, @"SELECT COUNT(1) FROM filter_state_audit WHERE workbook_name = '<WORKBOOK>' AND view_name = '<VIEW>'");
                return ((int)cmd.ExecuteScalar()) > 0;
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
