using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTableWriter.Connection;
using Npgsql;
using System.IO;

namespace PalMon.Output
{
    class PostgresOutput : IOutput
    {

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

            // connect to the db
            Console.Out.WriteLine(String.Format("Connecting to results database", connectionString));
            connection.Open();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;

                // Retrieve all rows
                cmd.CommandText = "SELECT 'hello'";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0));
                    }
                }
            }
        }

        #region IOutput implementation
        public void Write(string csvFile, ServerLogRow[] rows)
        {
            DoBulkCopy(DataConverter.Convert(rows));
        }

        public void Write(string csvFile, ThreadInfoRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Write(string csvFile, FilterStateChangeRow[] rows)
        {
            DoBulkCopy(DataConverter.Convert(rows));
        }

        #endregion

        #region Bulk copy handling
        /// <summary>
        /// Helper to do a bulk copy
        /// </summary>
        /// <param name="conversionResult"></param>
        private void DoBulkCopy(ConvertedRows conversionResult)
        {
            // connect to the db if needed
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
                Console.Out.WriteLine(String.Format("Reconnecting to results database."));
            }

            var statusLine = String.Format("BULK COPY of {0} - {1} rows",
                conversionResult.TableName,
                conversionResult.Rows.Count());

            var copyString = String.Format("COPY {0} ({1}) FROM STDIN",
                conversionResult.TableName,
                String.Join(", ", conversionResult.Columns));

            LoggingHelpers.TimedLog(statusLine, () =>
            {
                using (var writer = connection.BeginTextImport(copyString))
                {
                    foreach (var r in conversionResult.Rows)
                    {
                        writer.WriteLine(ToTSVLine(r));
                    }
                }
            });
        }

        /// <summary>
        /// Converts a list of fields into a tab-separated values string
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static string ToTSVLine(object[] row)
        {
            return String.Join("\t", row.Select(x => x.ToString().Replace("\n", "\\n").Replace("\t", "    ").Replace("\r", "\\r")));
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
