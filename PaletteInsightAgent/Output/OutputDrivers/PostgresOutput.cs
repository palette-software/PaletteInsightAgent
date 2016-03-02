using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using System.IO;
using System.Data;
using NLog;
using System.Globalization;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.LogPoller;
using PaletteInsightAgent.Sampler;
using PaletteInsightAgent.ThreadInfoPoller;

namespace PaletteInsightAgent.Output
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
            return DoBulkCopyWrapper(csvFiles);
        }

        #endregion

        #region datatable bulk ops

        private OutputWriteResult DoBulkCopyWrapper(IList<string> fileNames)
        {
            // first try to copy all files in a batch
            try
            {
                return DoBulkCopy(fileNames);
            }
            catch(Exception e)
            {
                // check if the exception is one that makes us unable to continue
                if (PostgresExceptionChecker.ExceptionIsFatalForBatch(e))
                {
                    // We have to add all files to the failed file list.
                    // We make a copy of the list here just to be on the safe side
                    // and the original list gets modified along the way
                    return new OutputWriteResult { failedFiles = new List<string>(fileNames) };
                }

                // check if we have to re-try the whole batch later
                if (PostgresExceptionChecker.ExceptionIsTemporaryForBatch(e))
                {
                    return new OutputWriteResult();
                }

                // if the exception is session fatal, move the unsent files to the unsent list
                // as we may recover them on next launch
                if (PostgresExceptionChecker.ExceptionIsSessionFatalForBatch(e))
                {
                    return new OutputWriteResult { unsentFiles = new List<string>(fileNames) };
                }


                // we should be able to retry the files if the exception isnt fatal
                // for the whole batch
                return OutputWriteResult.Aggregate( fileNames,(fileName) => DoSingleFileCopy(fileName));
            }

        }


        /// <summary>
        /// Do a single file copy, but this time catch all errors (this method calls DoBulkCopy()
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if the file was successfully copied to the database, or false if it failed</returns>
        private OutputWriteResult DoSingleFileCopy(string fileName)
        {
            var fileNameList = new List<string> { fileName };
            try
            {
                return DoBulkCopy(fileNameList);
            }
            catch (Exception e)
            {
                // if the exception is fatal, add the file to the failed list
                if (PostgresExceptionChecker.ExceptionIsFatalForFile(e))
                {
                    Log.Error(e, "Fatal exception encountered while trying to send '{0}' to the database", fileName);
                    return new OutputWriteResult{ failedFiles = fileNameList };
                }

                // if the exception is temporary, do not add to any lists, so we may
                // re-try it later
                if (PostgresExceptionChecker.ExceptionIsTemporaryForFile(e))
                {
                    return new OutputWriteResult();
                }

                // if the exception is session fatal, move the unsent file to the unsent list
                // as we may recover them on next launch
                if (PostgresExceptionChecker.ExceptionIsSessionFatalForFile(e))
                {
                    return new OutputWriteResult { unsentFiles = fileNameList };
                }

                Log.Error(e, "Unable to determine if exception is fatal for the file '{0}' or not: {1}", fileName, e);
                // remove this file from the errors list
                return new OutputWriteResult { failedFiles = fileNameList };
            }
        }

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

                    // Log the number of rows written here instead of spilling this aspect
                    // to LoggingHelpers
                    Log.Info("Total rows written: {0}", rowsWritten);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error during writing to the database: {0}", e);
                    // if anything went wrong, we should roll back the transaction
                    if (copyTransaction != null) copyTransaction.Rollback();
                    // Re-throw the exception here, we can pick up the error later.
                    // Since LoggingHelpers contains no try-catch blocks, the exception should fall
                    // right through.
                    throw;
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
