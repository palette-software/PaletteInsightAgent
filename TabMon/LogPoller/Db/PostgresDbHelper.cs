using System;
using System.Data;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller.Db
{
    public class PostgresDbQueries : IDbQueries
    {
        // Our query goes from the oldest to the newest unknown entries
        public string SELECT_FSA_TO_UPDATE_SQL { get { return @"SELECT id, sess, ts FROM filter_state_audit WHERE workbook= '<WORKBOOK>' AND view='<VIEW>' AND ts < @ts AND ts > @min_ts ORDER BY ts asc LIMIT 100"; } }

        public string UPDATE_FSA_SQL { get { return @"UPDATE filter_state_audit SET workbook=@workbook, view=@view, user_ip=@user_ip WHERE id = @id"; } }

        public string HAS_FSA_TO_UPDATE_SQL { get { return @"SELECT COUNT(1) FROM filter_state_audit WHERE workbook = '<WORKBOOK>' AND view = '<VIEW>' AND ts < @ts AND ts > @min_ts"; } }

    }

    public class PostgresDbHelper : IDbHelper
    {
        public PostgresDbHelper() { }

        private static PostgresDbQueries queries = new PostgresDbQueries();
        public IDbQueries Queries { get { return queries; } }


        public IDbConnection ConnectTo(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        /// <summary>
        /// Create a new SQL command from an SQL statement.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public IDbCommand MakeSqlCommand(IDbConnection conn, string cmdText)
        {
            NpgsqlCommand cmd = null;
            try
            {
                cmd = new NpgsqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = (NpgsqlConnection)conn;
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
        public void AddSqlParameter(IDbCommand cmd, string name, object val)
        {
            var timestampParam = cmd.CreateParameter();
            timestampParam.ParameterName = "@" + name;
            timestampParam.Value = val;
            cmd.Parameters.Add(timestampParam);
        }

    }
}
