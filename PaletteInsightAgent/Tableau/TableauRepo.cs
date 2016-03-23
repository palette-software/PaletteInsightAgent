using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Npgsql;
using NLog;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using PaletteInsightAgent.Output;

namespace PaletteInsightAgent.RepoTablesPoller
{

    public struct ViewPath
    {
        public string workbook;
        public string view;

        public string ip;

        public static ViewPath make(string workbook, string view, string ip)
        {
            var p = new ViewPath();
            p.workbook = workbook;
            p.view = view;
            p.ip = ip;
            return p;
        }

        public bool isEmpty() { return workbook == null && view == null; }

        public static ViewPath Empty = make(null, null, null);
    }

    /// <summary>
    /// Interface class for the tableau repo
    /// </summary>
    public interface ITableauRepoConn : IDisposable
    {
        ViewPath getViewPathForVizQLSessionId(string vizQLSessionId, DateTime timestamp);
        DataTable GetTable(string tableName);
        DataTable GetStreamingTable(string tableName, string field, string filter, string from, out string newMax);
        DataTable GetIndices();
        DataTable GetSchemaTable();
        int getCoreCount();
    }

    public class Tableau9RepoConn : ITableauRepoConn
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private NpgsqlConnection connection;
        private readonly NpgsqlConnectionStringBuilder connectionStringBuilder;
        private object readLock = new object();

        public Tableau9RepoConn(IDbConnectionInfo db)
        {
            connectionStringBuilder =
                new NpgsqlConnectionStringBuilder()
                {
                    Host = db.Server,
                    Port = db.Port,
                    Username = db.Username,
                    Password = db.Password,
                    Database = db.DatabaseName
                };

            Log.Info("Connecting to Tableau Repo PostgreSQL:" + db.Server);
            OpenConnection();
            Log.Info("Connected to Tableau Repo...");

        }

        /// <summary>
        /// Open the database connection.
        /// </summary>
        void OpenConnection()
        {
            try
            {
                connection = new NpgsqlConnection(connectionStringBuilder);
                connection.Open();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to open database connection! Exception message: {0}", ex.Message);
                connection = null;
                return;
            }

            if (!IsConnectionOpen())
            {
                throw new DbConnectionException("Could not open connection to Tableau PostgreSQL repository.");
            }
        }


        /// <summary>
        /// Close the database connection.
        /// </summary>
        void CloseConnection()
        {
            connection.Close();
        }

        /// <summary>
        /// Returns true if database connection is in an open state.
        /// </summary>
        /// <returns></returns>
        bool IsConnectionOpen()
        {
            return connection != null && 
                   connection.State == ConnectionState.Open;
        }


        void reconnect()
        {
            try
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to close Tableau database connection! Exception message: {0}", ex.Message);
            }

            OpenConnection();
        }

        /// <summary>
        /// Returns the total number of cores allocated to the Tableau cluster represented by the repository.
        /// </summary>
        public int getCoreCount()
        {
            var query = "SELECT coalesce(sum(allocated_cores),0) FROM core_licenses;";
            long coreCount = runScalarQuery(query);
            Log.Info("Tableau total allocated cores: {0}", coreCount);
            return (int)coreCount;
        }

        private DataTable runQuery(string query)
        {
            DataTable table = new DataTable();
            lock (readLock)
            {
                if (!IsConnectionOpen())
                {
                    reconnect();
                }
                try
                {
                    using (var adapter = new NpgsqlDataAdapter(query, connection))
                    {
                        adapter.Fill(table);
                    }
                }
                catch (Npgsql.NpgsqlException e)
                {
                    Log.Error("Error while retreiving data from Tableau repository Query: {0} Exception: {1}", query, e);
                }
            }
            return table;
        }

        private long runScalarQuery(string query)
        {
            long max = 0;
            lock (readLock)
            {
                if (!IsConnectionOpen())
                {
                    reconnect();
                }
                try
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        // Insert some data
                        cmd.CommandText = query;
                        max = (long)cmd.ExecuteScalar();
                    };
                }
                catch (Npgsql.NpgsqlException e)
                {
                    Log.Error("Error while retreiving data from Tableau repository Query: {0} Exception: {1}", query, e);
                }
            }
            return max;
        }

        public DataTable GetSchemaTable()
        {
            var query = @"
                    SELECT n.nspname as schemaname, c.relname as tablename,
                    a.attname as columnname,
                    format_type(a.atttypid, a.atttypmod),
                    a.attnum
                    FROM pg_namespace n
                      JOIN pg_class c ON (n.oid = c.relnamespace)
                      JOIN pg_attribute a ON (c.oid = a.attrelid)
                      JOIN pg_type t ON (a.atttypid = t.oid)
                    WHERE 1 = 1
                    AND   nspname = 'public'
                    AND a.attnum > 0 /*filter out the internal columns*/
                    ORDER BY n.nspname,c.relname,a.attnum ASC";
            var table = runQuery(query);
            table.TableName = "metadata";
            return table;
        }

        public DataTable GetIndices()
        {
            var query = @"
                select
                   n.nspname as schema_name,
                   t.relname as table_name,
                   i.relname as index_name,
                   a.attname as column_name,
                  pg_get_indexdef(ix.indexrelid)
                from
                   pg_class t,
                   pg_class i,
                   pg_index ix,
                   pg_attribute a,
                   pg_namespace n
                where
                   t.oid = ix.indrelid
                   and i.oid = ix.indexrelid
                   and a.attrelid = t.oid
                   and a.attnum = ANY(ix.indkey)
                   and t.relkind = 'r'
                   and t.relnamespace = n.oid
                   and n.nspname = 'public'
                order by
                   t.relname,
                   i.relname
                ";
            var table = runQuery(query);
            table.TableName = "index";
            return table;
        }

        public DataTable GetTable(string tableName)
        {
            var query = String.Format("select * from {0}", tableName);
            var table = runQuery(query);
            table.TableName = tableName;
            return table;
        }

        private string GetMax(string tableName, string field, string filter)
        {
            var query = String.Format("select max({0}) from {1}", field, tableName);
            if (filter != null)
            {
                query = String.Format("{0} where {1}", query, filter);
            }
            var table = runQuery(query);
            // This query should return one field
            if (table.Rows.Count == 1 && table.Columns.Count == 1)
            {
                return table.Rows[0][0].ToString();
            }
            return null;
        }

        public DataTable GetStreamingTable(string tableName, string field, string filter, string from, out string newMax)
        {
            // At first determine the max until we can query
            newMax = GetMax(tableName, field, filter);

            // If we don't quit here we risk data being created before 
            // actually asking for it and not having maxId set correctly
            if (newMax == null || newMax == "")
            {
                return null;
            }

            var query = "";
            if (from == null)
            {
                // This means we have not yet sent anything. In this case we should just send the newest row.
                query = String.Format("select * from {0} where {1} = '{2}'", tableName, field, newMax);
            }
            else
            {
                query = String.Format("select * from {0} where {1} > '{2}' and {1} <= '{3}'", tableName, field, from, newMax);
            }

            if (filter != null)
            {
                query += String.Format(" and {0}", filter);
            }
            var table = runQuery(query);
            table.TableName = tableName;
            return table;
        }


        public ViewPath getViewPathForVizQLSessionId(string vizQLSessionId, DateTime timestamp)
        {
            if (String.IsNullOrWhiteSpace(vizQLSessionId)) return ViewPath.Empty;

            lock (readLock)
            {

                if (!IsConnectionOpen())
                {
                    reconnect();
                }

                using (var cmd = connection.CreateCommand())
                {

                    cmd.Connection = connection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT currentsheet, user_ip FROM http_requests WHERE vizql_session=@vizql_session_id AND created_at < @timestamp ORDER BY created_at DESC LIMIT 1";

                    var sessionIdParam = cmd.CreateParameter();
                    sessionIdParam.ParameterName = "@vizql_session_id";
                    sessionIdParam.Value = vizQLSessionId;
                    cmd.Parameters.Add(sessionIdParam);


                    var timestampParam = cmd.CreateParameter();
                    timestampParam.ParameterName = "@timestamp";
                    timestampParam.Value = timestamp;
                    cmd.Parameters.Add(timestampParam);

                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var sheet = reader["currentsheet"].ToString().Split(new char[] { '/' }, 2);
                                var ip = reader["user_ip"].ToString();
                                return ViewPath.make(sheet[0], sheet[1], ip);
                            }
                        }
                        return ViewPath.Empty;
                    }
                    catch (DbException ex)
                    {
                        Log.Error("Error getting the vizql information for session id={0}. Exception message: {1}", vizQLSessionId, ex.Message);
                        throw;
                    }
                    catch (IOException ioe)
                    {
                        Log.Error("IO Exception caught while getting view path for vizql session: {0}. Exception message: {1}", vizQLSessionId, ioe.Message);
                        try
                        {
                            connection.Close();
                        }
                        catch (Exception e)
                        {
                            Log.Error("Exception caught while closing crippled connection for vizql session: {0}. Exception message: {1}", vizQLSessionId, e.Message);
                        }
                        finally
                        {
                            connection = null;
                        }
                    }
                }
            }

            // Fallback to empty view path
            return ViewPath.Empty;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    connection.Close();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        ~Tableau9RepoConn()
        {
            Dispose(false);
        }
        #endregion

    }

    #region Custom Exceptions

    /// <summary>
    /// Custom exception used to encapsulate the various database connection error conditions we may encounter.
    /// </summary>
    [Serializable]
    public class DbConnectionException : DbException
    {
        public DbConnectionException(string message)
            : base(message)
        { }

        protected DbConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    #endregion Custom Exceptions
}
