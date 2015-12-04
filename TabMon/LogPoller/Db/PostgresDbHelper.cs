using System;
using System.Data;
using Npgsql;

namespace TabMon.LogPoller.Db
{
    public class PostgresDbHelper : IDbHelper
    {
        public PostgresDbHelper() { }



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
