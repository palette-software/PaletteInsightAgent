using System;
using System.Data;
using System.Collections.Generic;
using log4net;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller.Db
{
    class SqlConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IDbHelper helper;
        private IDbConnection connection;

        private SqlConnection(IDbHelper h, IDbConnection conn)
        {
            helper = h;
            connection = conn;
        }

        #region Public API

        /// <summary>
        /// Run a query on this connection.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public SqlQuery query(string queryString)
        {
            return new SqlQuery(connection, helper, queryString);
        }


        /// <summary>
        /// Public API to connect to a database in a safe manner
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="connectionString"></param>
        /// <param name="whatToDo"></param>
        public static void WithConnection(IDbHelper helper, string connectionString, Action<SqlConnection> whatToDo)
        {
            using (var conn = helper.ConnectTo(connectionString))
            {
                conn.Open();
                var sqlConnection = new SqlConnection(helper, conn);
                try
                {
                    whatToDo(sqlConnection);
                }
                catch (Exception e)
                {
                    Log.Error(String.Format("Exception during sql connection to: {0}", connectionString), e);
                }
            }

        }

        #endregion
    }

    /// <summary>
    /// Helper class to build SQL queries and run them in an exception-catching manner.
    /// </summary>
    class SqlQuery
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string queryString;
        private Dictionary<string, object> sqlParams = new Dictionary<string, object>();

        private IDbConnection connection;
        private IDbHelper dbHelper;

        private Action<IDataReader> onResults;
        private Action<object> onScalar;
        private Action<Exception> onFailiure;

        #region Constructors

        public SqlQuery(IDbConnection conn, IDbHelper dbHelper)
        {
            this.connection = conn;
            this.dbHelper = dbHelper;
        }


        /// <summary>
        /// Shorthand constructor for creating a query from a query string.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbHelper"></param>
        /// <param name="queryString"></param>
        public SqlQuery(IDbConnection conn, IDbHelper dbHelper, string queryString)
        {
            this.queryString = queryString;
            this.connection = conn;
            this.dbHelper = dbHelper;
        }

        #endregion

        #region Public API

        public SqlQuery Query(string queryString) { this.queryString = queryString; return this; }


        /// <summary>
        /// Add a param to the query
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public SqlQuery Param(string name, object value)
        {
            sqlParams.Add(name, value);
            return this;
        }


        /// <summary>
        /// Add multiple parameters to the query
        /// </summary>
        /// <param name="paramsToMerge"></param>
        /// <returns></returns>
        public SqlQuery Params(IDictionary<string, object> paramsToMerge)
        {
            // Since C# contains no Dictionary#merge, we have to do it the ugly way
            foreach (var kv in paramsToMerge)
                Param(kv.Key, kv.Value);

            return this;
        }
        #endregion

        #region Callback handlers

        /// <summary>
        /// Assing a delegate that runs when a Query is successful.
        /// </summary>
        /// <param name="onResults"></param>
        /// <returns></returns>
        public SqlQuery OnResults(Action<IDataReader> onResults) { this.onResults = onResults; return this; }

        /// <summary>
        /// <summary>
        /// Assing a delegate that runs when a scalar Query is successful.
        /// </summary>
        /// <param name="onResults"></param>
        /// <returns></returns>
        public SqlQuery OnScalar(Action<object> onScalar) { this.onScalar = onScalar; return this; }


        /// <summary>
        /// Assing a delegate that runs when a Query is successful.
        /// </summary>
        /// <param name="onResults"></param>
        /// <returns></returns>
        public SqlQuery OnError(Action<Exception> onFail) { this.onFailiure = onFail; return this; }
        #endregion

        #region Running the statement

        /// <summary>
        /// Run this SQL statement as a non-query
        /// </summary>
        public void RunStatement() { DoRun(DoRunNonQuery); }

        /// <summary>
        /// Run the query as a query and call the delegate with a reader
        /// </summary>
        public void RunQuery() { DoRun(DoRunQuery); }

        /// <summary>
        /// Runs a scalar-returning query
        /// </summary>
        public void RunScalar() { DoRun(DoRunScalar); }

        #endregion

        #region private helpers

        /// <summary>
        /// Internal helper containing the logic for creating and error handling of a query.
        /// </summary>
        /// <param name="runWhat"></param>
        private void DoRun(Action<IDbCommand> runWhat)
        {
            using (var q = dbHelper.MakeSqlCommand(connection, queryString))
            {
                AddParameters(q);
                // Run the delegate
                try
                {
                    runWhat(q);
                }
                catch (Exception e)
                {
                    Log.Error(String.Format("Exception during query: {0}", queryString), e);
                    onFailiure(e);
                }
            }
        }


        /// <summary>
        /// Internal helper for reader-returning queries
        /// </summary>
        /// <param name="q"></param>
        private void DoRunQuery(IDbCommand q)
        {
            using (var reader = q.ExecuteReader())
            {
                onResults(reader);
            }
        }

        /// <summary>
        /// Internal helper for scalar-returning queries
        /// </summary>
        /// <param name="q"></param>
        private void DoRunScalar(IDbCommand q)
        {
            onScalar(q.ExecuteScalar());
        }

        /// <summary>
        /// Internal helper for non-query SQL statements
        /// </summary>
        /// <param name="q"></param>
        private void DoRunNonQuery(IDbCommand q)
        {
            q.ExecuteNonQuery();
        }


        /// <summary>
        /// Helper that adds all SQL parameters to the IDbCommand
        /// </summary>
        /// <param name="q"></param>
        private void AddParameters(IDbCommand q)
        {
            // Add all parameters
            foreach (var param in sqlParams)
            {
                dbHelper.AddSqlParameter(q, param.Key, param.Value);
            }
        }

        #endregion
    }

}
