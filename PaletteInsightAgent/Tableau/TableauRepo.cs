﻿using System;
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
using System.Net.Sockets;
using PaletteInsight.Configuration;

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
    }

    public class Tableau9RepoConn : ITableauRepoConn
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private NpgsqlConnection connection;
        private readonly NpgsqlConnectionStringBuilder connectionStringBuilder;
        private object readLock = new object();

        public Tableau9RepoConn(DbConnectionInfo db)
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

        private object queryWithReconnect(Func<object> query, object def)
        {
            // If server got restarted we get IOException for the first time and there is no
            // other way to detect this but sending the query. This is why we have the for loop
            for (int i= 0; i < 2; i++)
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
                    Log.Error("NPGSQL exception while retreiving data from Tableau repository Query: {0} Exception: {1}", query, e);
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
                return (DataTable)queryWithReconnect(() => {
                    using (var adapter = new NpgsqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }, new DataTable());
            }
        }

        private long runScalarQuery(string query)
        {
            lock (readLock)
            {
                return (long)queryWithReconnect(() =>
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        // Insert some data
                        cmd.CommandText = query;
                        long max = (long)cmd.ExecuteScalar();
                        return max;
                    };
                }, 0);
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

        public DataTable GetStreamingTable(string tableName, RepoTable table, string from, out string newMax)
        {
            // At first determine the max until we can query
            newMax = GetMax(tableName, table.Field, table.Filter);

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
