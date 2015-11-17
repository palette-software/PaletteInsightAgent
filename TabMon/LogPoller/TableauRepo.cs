﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Npgsql;
using log4net;
using System.Reflection;
using System.Runtime.Serialization;

namespace TabMon.LogPoller
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

    }

    public class Tableau9RepoConn : ITableauRepoConn
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private NpgsqlConnection connection;

        public Tableau9RepoConn(string host, int port, string username, string password, string database)
        {
            var connectionString =
                new NpgsqlConnectionStringBuilder()
                {
                    Host = host,
                    Port = port,
                    UserName = username,
                    Password = password,
                    Database = database
                };

            Log.Info("Connecting to Tableau Repo PostgreSQL:" + host);
            connection = new NpgsqlConnection(connectionString);
            OpenConnection();
            Log.Info("Connected to Tableau Repo...");

        }

        /// <summary>
        /// Open the database connection.
        /// </summary>
        void OpenConnection()
        {
            connection.Open();

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
            return connection.State == ConnectionState.Open;
        }


        public ViewPath getViewPathForVizQLSessionId(string vizQLSessionId, DateTime timestamp)
        {
            if (String.IsNullOrWhiteSpace(vizQLSessionId)) return ViewPath.Empty;


            using (var cmd = connection.CreateCommand())
            {

                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM http_requests WHERE vizql_session=@vizql_session_id AND created_at < @timestamp ORDER BY created_at ASC LIMIT 1";

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
                    Log.Error(String.Format("Error getting the vizql information for session id={0}", vizQLSessionId), ex);
                    throw;
                }
            }

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
