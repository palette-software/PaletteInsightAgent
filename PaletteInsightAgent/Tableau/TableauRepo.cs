﻿using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using Npgsql;
using NLog;
using PaletteInsightAgent.Output;
using PaletteInsightAgent.Configuration;

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
        DataTable GetTable(string tableName);
        DataTable GetStreamingTable(string tableName, RepoTable table, string from, out string newMax);
        DataTable GetIndices();
        DataTable GetSchemaTable();
        int getCoreCount();
        bool isInRecoveryMode();
    }

    public class Tableau9RepoConn : ITableauRepoConn
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private NpgsqlConnection connection;
        private readonly NpgsqlConnectionStringBuilder connectionStringBuilder;
        private object readLock = new object();

        private int streamingTablesPollLimit;

        public Tableau9RepoConn(DbConnectionInfo db, int streamingTablesPollLimit)
        {
            if (db == null)
            {
                Log.Error("DB connection info is NULL while making Tableau repo connection!");
                return;
            }

            this.streamingTablesPollLimit = streamingTablesPollLimit;

            connectionStringBuilder =
                new NpgsqlConnectionStringBuilder()
                {
                    // Always try to connect to the Tableau repository from the machine where the target
                    // Tableau repository resides
                    Host = "localhost",
                    Port = db.Port,
                    Username = db.Username,
                    Password = db.Password,
                    Database = db.DatabaseName,
                    SslMode = SslMode.Prefer,
                    // Trust invalid certificates as well
                    TrustServerCertificate = true
                };
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
                Log.Info("Connected to Tableau Repo...");
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to open database connection! Exception: {0}", ex.GetType());
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
            long coreCount = runScalarQuery<long>(query);
            Log.Info("Tableau total allocated cores: {0}", coreCount);
            return (int)coreCount;
        }

        public bool isInRecoveryMode()
        {
            var query = "SELECT pg_is_in_recovery();";
            bool isInRecovery = runScalarQuery<bool>(query);
            Log.Info("Tableau is {0}in recovery mode", isInRecovery ? "" : "not ");
            return isInRecovery;
        }

        private object queryWithReconnect(Func<object> query, object def, string sqlStatement)
        {
            // If server got restarted we get IOException for the first time and there is no
            // other way to detect this but sending the query. This is why we have the for loop
            for (int i = 0; i < 2; i++)
            {
                if (!IsConnectionOpen())
                {
                    reconnect();
                }
                try
                {
                    return query();
                }
                catch (Npgsql.NpgsqlException e)
                {
                    Log.Error("NPGSQL exception while retreiving data from Tableau repository Query: {0} Exception: {1}", sqlStatement, e);
                    break;
                }
                catch (System.IO.IOException e)
                {
                    Log.Warn("Postgres Server was restarted. Reconnecting.", e);
                }
            }
            return def;
        }

        private DataTable runQuery(string query)
        {
            lock (readLock)
            {
                return (DataTable)queryWithReconnect(() =>
                {
                    using (var adapter = new NpgsqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }, new DataTable(), query);
            }
        }

        private T runScalarQuery<T>(string query)
        {
            lock (readLock)
            {
                return (T)queryWithReconnect(() =>
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        // Insert some data
                        cmd.CommandText = query;
                        T max = (T)cmd.ExecuteScalar();
                        return max;
                    };
                }, 0, query);
            }
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

        internal string GetMaxQuery(string tableName, string field, string filter, string prevMax)
        {
            var maxFilterClause = prevMax != null ? $"and {field} > '{prevMax}'" : "";
            var filterClause    = filter  != null ? $"and {filter}"              : "";

            // Limit result to prevent System.OutOfMemoryException in Agent. But..
            // if there is no 'prevMax', let's select the max record from the table. We need to skip
            // this limit in that case. If 'prevMax' is missing, it means that the agent is starting up
            // (this is why local max ID is missing) and the agent has no connection to the Insight
            // Server (this is why max ID coming from the Server is missing).
            var limitClause     = maxFilterClause != "" ? $"limit {this.streamingTablesPollLimit}" : "";

            return $@"
                select max({field})
                from
                    (
                    select {field}
                    from {tableName}
                        where 1 = 1
                        {maxFilterClause}
                        {filterClause}
                        order by {field} asc
                        {limitClause}
                    ) as iq
                ;";
        }

        private string GetMax(string tableName, string field, string filter, string prevMax)
        {
            var query = GetMaxQuery(tableName, field, filter, prevMax);
            var table = runQuery(query);
            // This query should return one field
            if (table.Rows.Count == 1 && table.Columns.Count == 1)
            {
                // TrimEnd removes trailing newline ( + whitespaces )
                return StringifyMax(table.Rows[0][0]).TrimEnd();
            }
            return null;
        }

        internal static string StringifyMax(object max)
        {
            if (max is DateTime)
            {
                // "u" means Universal sortable date/time pattern. This way we can
                // avoid problems with different date/time patterns for example like
                // dd/mm/yyyy and mm/dd/yyyy, which can result ambigous or invalid dates.
                return ((DateTime)max).ToString("u");
            }
            return max.ToString();
        }

        public DataTable GetStreamingTable(string tableName, RepoTable table, string from, out string newMax)
        {
            // At first determine the max until we can query
            newMax = GetMax(tableName, table.Field, table.Filter, from);

            // If we don't quit here we risk data being created before
            // actually asking for it and not having maxId set correctly
            if (newMax == null || newMax == "")
            {
                return null;
            }

            var query = "";
            // This means we have not yet sent anything. In this case we should just send the newest row or the whole table
            // depending on the Table configuration
            if (from == null)
            {
                if (table.HistoryNeeded)
                {
                    query = String.Format("select * from {0}", tableName);
                }
                else
                {
                    query = String.Format("select * from {0} where {1} = '{2}'", tableName, table.Field, newMax);
                }
            }
            else
            {
                query = String.Format("select * from {0} where {1} > '{2}' and {1} <= '{3}'", tableName, table.Field, from, newMax);
            }

            if (table.Filter != null)
            {
                query += String.Format(" and {0}", table.Filter);
            }
            var dataTable = runQuery(query);
            dataTable.TableName = tableName;
            return dataTable;
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
