using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTableWriter.Connection;
using Npgsql;
using System.IO;
using System.Data;
using NLog;

namespace PalMon.Output
{
    class PostgresOutput : IOutput
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IDbConnectionInfo resultDatabase;
        private string connectionString;

        private NpgsqlConnection connection;

        public PostgresOutput(IDbConnectionInfo resultDatabase)
        {
            this.resultDatabase = resultDatabase;


            // Generate a postgres-compatible connection string
            connectionString = String.Format("Host={0};Port={1};Username={2};Password={3};Database={4}",
                resultDatabase.Server, resultDatabase.Port,
                resultDatabase.Username, resultDatabase.Password,
                resultDatabase.DatabaseName
                );

            connection = new NpgsqlConnection(connectionString);

        }

        #region IOutput implementation
        public void Write(string csvFile, DataTable rows)
        {
            DoBulkCopy(rows);
        }

        #endregion

        #region datatable bluk ops

        /// <summary>
        /// Helper to do a bulk copy
        /// </summary>
        /// <param name="conversionResult"></param>
        private void DoBulkCopy(DataTable rows)
        {
            ReconnectoToDbIfNeeded();

            var statusLine = String.Format("BULK COPY of {0} - {1} rows", rows.TableName, rows.Rows.Count);
            string copyString = CopyStatementFor(rows);


            LoggingHelpers.TimedLog(Log, statusLine, () =>
            {
                using (var writer = connection.BeginTextImport(copyString))
                {

                    var columnCount = rows.Columns.Count;

                    foreach (DataRow rowToWrite in rows.Rows)
                    {
                        // Output the joined row
                        writer.WriteLine(String.Join("\t", ToTSVLine(rowToWrite.ItemArray)));
                    }
                }
            });
        }
        
        private static string ToTSVLine(object[] row)
        {
            return String.Join("\t", row.Select(x => x.ToString().Replace("\n", "\\n").Replace("\t", "    ").Replace("\r", "\\r")));
        }

        /// <summary>
        /// Returns a postgres copy statement for the datatable
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private static string CopyStatementFor(DataTable rows)
        {

            // build a list of column names
            var columnNames = new List<string>();
            foreach (DataColumn col in rows.Columns)
            {
                columnNames.Add(col.ColumnName);
            }

            var copyString = String.Format("COPY {0} ({1}) FROM STDIN", rows.TableName, String.Join(", ", columnNames));
            return copyString;
        }

        private void ReconnectoToDbIfNeeded()
        {
            // connect to the db if needed
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
                Log.Info("Reconnecting to results database.");
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PostgresOutput() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
