﻿using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using System.IO;
using System.Data;
using NLog;
using System.Globalization;
using PalMon.Helpers;
using PalMon.LogPoller;
using PalMon.Sampler;
using PalMon.ThreadInfoPoller;

namespace PalMon.Output
{
    class PostgresOutput : IOutput
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IDbConnectionInfo resultDatabase;
        private string connectionString;
        private NpgsqlConnection connection;
        private Dictionary<string, Func<DataTable>> tableCreators = new Dictionary<string, Func<DataTable>>
        {
            { LogTables.SERVERLOGS_TABLE_NAME,          LogTables.makeServerLogsTable },
            { LogTables.FILTER_STATE_AUDIT_TABLE_NAME,  LogTables.makeFilterStateAuditTable},
            { ThreadTables.TABLE_NAME,                  ThreadTables.makeThreadInfoTable},
            { CounterSampler.TABLE_NAME,                CounterSampler.makeCounterSamplesTable}
        };

        public PostgresOutput(IDbConnectionInfo resultDatabase)
        {
            this.resultDatabase = resultDatabase;


            // Generate a postgres-compatible connection string
            connectionString = String.Format("Host={0};Port={1};Username={2};Password={3};Database={4};CommandTimeout={5}",
                resultDatabase.Server, resultDatabase.Port,
                resultDatabase.Username, resultDatabase.Password,
                resultDatabase.DatabaseName,
                resultDatabase.CommandTimeout
                );

            connection = new NpgsqlConnection(connectionString);

        }

        #region IOutput implementation

        public OutputWriteResult Write(IList<string> csvFiles)
        {
            return DoBulkCopy(csvFiles);
        }

        #endregion

        #region datatable bulk ops

        /// <summary>
        /// Helper to do a bulk copy
        /// </summary>
        /// <param name="conversionResult"></param>
        private OutputWriteResult DoBulkCopy(IList<string> fileNames)
        {
            if (fileNames.Count <= 0)
            {
                // There are no files to process.
                return new OutputWriteResult();
            }


            ReconnectoToDbIfNeeded();
            var tableName = DBWriter.GetTableName(fileNames[0]);

            // Make sure we have the proper table
            if (!tableCreators.ContainsKey(tableName))
            {
                Log.Error("Unexpected table name: {0}", tableName);
                // return with all files as failed files
                return new OutputWriteResult { failedFiles = new List<string>(fileNames) };
            }

            // at this point we should have a nice table
            var table = tableCreators[tableName]();

            UpdateTableStructureFor(table);

            var statusLine = String.Format("BULK COPY of {0} (Number of files: {1})", tableName, fileNames.Count);
            string copyString = CopyStatementFor(fileNames[0]);

            // create storage for the successfully uploaded csv filenames
            var outputResult = new OutputWriteResult();

            LoggingHelpers.TimedLog(Log, statusLine, () =>
            {
                int rowsWritten = 0;
                // begin a transaction before any insert takes place
                var copyTransaction = connection.BeginTransaction();

                // we wrap the copy in a try/catch block so we can roll back the transaction in case of errors
                try
                {

                    using (var writer = connection.BeginTextImport(copyString))
                    {
                        // Files contents for the same table are sent in one bulk
                        foreach (var fileName in fileNames)
                        {
                            if (!copyString.Equals(CopyStatementFor(fileName)))
                            {
                                Log.Error("Skipping file since CSV header is not matching with others in file: {0}", fileName);
                                continue;
                            }

                            using (var reader = new StreamReader(fileName))
                            {
                                // Skip the first line of the CSV file as it only contains the CSV header
                                var lineRead = reader.ReadLine();

                                while (true)
                                {
                                    lineRead = reader.ReadLine();
                                    if (lineRead == null)
                                    {
                                        // End of file
                                        break;
                                    }
                                    rowsWritten++;
                                    writer.WriteLine(lineRead);
                                }
                            }
                        }
                    }
                    // commit the transaction after all rows are written (after writer.Dispose() is called)
                    copyTransaction.Commit();
                    // since we do all the copy in a single run, we add all files to the list of processed ones here, but
                    // we store every file name in case we ever want to add by-file-error-handling later
                    outputResult.successfullyWrittenFiles.AddRange(fileNames);

                    return rowsWritten;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error during writing to the database: {0}", e);
                    // if anything went wrong, we should roll back the transaction
                    if (copyTransaction != null) copyTransaction.Rollback();
                    // in case of errors we have inserted 0 rows thanks to the transaction
                    // but since we have errors in the insertion (which may come from
                    // connectivity issues), we dont add any files to either
                    // the successful or the error list, so the files
                    // will be re-tried on next invocation
                    return 0;
                }
            });
            // return the list of processed files, which should for now be either empty or
            // contain all the input file names
            return outputResult;

        }

        /// <summary>
        /// Converts an array of objects to a TSV-line
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static string ToTSVLine(object[] row)
        {
            return String.Join("\t", row.Select(ToTSVValue));
        }

        /// <summary>
        /// Converters for types for the tsv output
        /// </summary>
        private static Dictionary<Type, Func<object, string>> tsvTypeSwitch = new Dictionary<Type, Func<object, string>>
        {
            {typeof(int), (o) => ((int)o).ToString() },
            {typeof(long), (o) => ((long)o).ToString() },
            {typeof(float), (o)=> ((float)o).ToString("F6", CultureInfo.InvariantCulture) },
            {typeof(double), (o)=> ((double)o).ToString("F12", CultureInfo.InvariantCulture) },
            {typeof(DateTime), (o) => ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss.fff") },
            // escape a string properly:
            // http://www.postgresql.org/docs/9.4/static/sql-copy.html
            // Backslash characters (\) can be used in the COPY data to quote data
            // characters that might otherwise be taken as row or column delimiters. 
            // In particular, the following characters must be preceded by a backslash
            // if they appear as part of a column value: backslash itself, newline,
            // carriage return, and the current delimiter character.
            {typeof(string), (o) => ((string)o)
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "    ")
            },
            {typeof(DBNull), (o) => "" }
        };

        /// <summary>
        ///  Tries to convert a value to TSV
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string ToTSVValue(object o)
        {
            var t = o.GetType();

            if (!tsvTypeSwitch.ContainsKey(t))
            {
                Log.Info("Trying type converter for {0}", t);
                throw new ArgumentException(String.Format("Cannot find type converter for:{0}", t));
            }

            return tsvTypeSwitch[t](o);
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
                columnNames.Add(String.Format("\"{0}\"", col.ColumnName));
            }

            var copyString = String.Format("COPY {0} ({1}) FROM STDIN", rows.TableName, String.Join(", ", columnNames));
            return copyString;
        }

        public static string CopyStatementFor(string fileName)
        {
            var tableName = DBWriter.GetTableName(fileName);
            if (tableName == "")
            {
                Log.Error("Invalid fileName format when creating CopyStatement.");
                return "";
            }

            // first row contains column names
            string columnNames = File.ReadLines(fileName).First(); // gets the first line from file.
            var copyString = String.Format("COPY {0} ({1}) FROM STDIN WITH CSV", tableName, columnNames);
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


        #region Structure modifications

        // Map of System types -> Postgres types.
        protected readonly IReadOnlyDictionary<string, string> systemToPostgresTypeMap
            = new Dictionary<string, string>()
            {
                { "System.Boolean", "boolean" },
                { "System.Byte", "smallint" },
                { "System.Char", "character(1)" },
                { "System.DateTime", "timestamp" },
                { "System.DateTimeOffset", "timestamp with time zone" },
                { "System.Decimal", "numeric" },
                { "System.Double", "double precision" },
                { "System.Int16", "smallint" },
                { "System.Int32", "integer" },
                { "System.Int64", "bigint" },
                { "System.Single", "float8" },
                { "System.String", "text" },
            };

        /// <summary>
        /// Maps the name of a C# System type to a Postgres type.
        /// </summary>
        /// <param name="systemType">The name of the System type.</param>
        /// <param name="allowDbNull">Flag indicating whether the input type is nullable.</param>
        /// <returns>Postgres type that correlates to the given System type.</returns>
        public string MapToDbType(string systemType, bool allowDbNull = true)
        {
            string pgType;
            if (!systemToPostgresTypeMap.ContainsKey(systemType))
            {
                pgType = "text";
            }
            else
            {
                pgType = systemToPostgresTypeMap[systemType];
            }

            if (!allowDbNull)
            {
                pgType = String.Format("{0} NOT NULL", pgType);
            }

            return pgType;
        }

        private void UpdateTableStructureFor(DataTable aTable)
        {
            ReconnectoToDbIfNeeded();
            var tableName = aTable.TableName;

            // figure out if the table exists
            if (!TableExists(tableName))
            {
                // create the table
                CreateTable(aTable);
            }
            else
            {
                HashSet<string> columnsExisting = GetDbColumnNames(aTable);

                foreach (DataColumn col in aTable.Columns)
                {
                    if (columnsExisting.Contains(col.ColumnName))
                        continue;

                    Log.Info("Adding column {0} to table {1}", col.ColumnName, aTable.TableName);

                    var sql = String.Format("ALTER TABLE \"{0}\" ADD COLUMN \"{1}\" {2}", tableName, col.ColumnName, MapToDbType(col.DataType.ToString()));

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                }
            }
        }

        /// <summary>
        /// Gets the names of all DB columns
        /// </summary>
        /// <param name="aTable"></param>
        /// <returns></returns>
        private HashSet<string> GetDbColumnNames(DataTable aTable)
        {
            var columnsExisting = new HashSet<string>();
            var sql = String.Format(@"SELECT
                                      column_name as name,
                                      data_type as type,
                                      is_nullable as nullable
                                    FROM information_schema.columns
                                    WHERE table_schema = 'public'
                                      AND table_name = '{0}'", aTable.TableName);
            using (var cmd = new NpgsqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    columnsExisting.Add(reader["name"].ToString());
                }
            }

            return columnsExisting;
        }

        /// <summary>
        /// Creates a table from the description in a DataTable
        /// </summary>
        /// <param name="aTable"></param>
        private void CreateTable(DataTable aTable)
        {
            var columnTypes = new List<string>();

            foreach (DataColumn col in aTable.Columns)
            {
                columnTypes.Add(String.Format("\"{0}\" {1}", col.ColumnName, MapToDbType(col.DataType.ToString(), true)));
            }

            var sql = String.Format("CREATE TABLE {0}({1})", aTable.TableName, String.Join(", ", columnTypes));

            Log.Info("Creating DB table: {0}", sql);
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Does the given table exist in the database?
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool TableExists(string tableName)
        {
            bool output;
            using (var cmd = new NpgsqlCommand(string.Format("SELECT * FROM pg_tables WHERE tablename='{0}'", tableName), connection))
            {
                var result = cmd.ExecuteScalar();
                output = !(result == null);
            }

            return output;
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
