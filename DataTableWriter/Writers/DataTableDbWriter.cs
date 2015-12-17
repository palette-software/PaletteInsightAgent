using DataTableWriter.Adapters;
using DataTableWriter.Connection;
using DataTableWriter.Drivers;
using log4net;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

namespace DataTableWriter.Writers
{
    /// <summary>
    /// Contains functionality for writing DataTable objects to a database.
    /// </summary>
    public class DataTableDbWriter : IDataTableWriter
    {
        protected IDbAdapter Adapter { get; set; }
        protected DbTableInitializationOptions tableInitializationOptions;
        /// <summary>
        /// Keep track of tables already initialized
        /// </summary>
        protected HashSet<string> isTableInitialized;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object DbWriteLock = new object();
        private bool disposed;

        public DataTableDbWriter(DbDriverType driverType, IDbConnectionInfo connectionInfo, DbTableInitializationOptions tableInitializationOptions = default(DbTableInitializationOptions))
        {
            Adapter = new DbAdapter(driverType, connectionInfo);
            isTableInitialized = new HashSet<string>();
            this.tableInitializationOptions = tableInitializationOptions;
        }

        ~DataTableDbWriter()
        {
            Dispose(false);
        }

        public string Name
        {
            get { return String.Format("Database Writer ({0})", Adapter.Driver.Name); }
        }

        private const int BATCH_SIZE = 1000;

        public void Write(DataTable table)
        {
            Log.Debug(String.Format("Writing {0} {1} to database..", table.Rows.Count, "record".Pluralize(table.Rows.Count)));

            // Reopen connection, if it has closed for some reason.
            if (!Adapter.IsConnectionOpen())
            {
                Adapter.OpenConnection();
            }

            // Manage dynamic table creation/management, if requested.
            var isInitialized = isTableInitialized.Contains(table.TableName);
            if (!isInitialized)
            {
                DbTableManager.InitializeTable(Adapter, table, tableInitializationOptions);
                isTableInitialized.Add(table.TableName);
            }


            int remainingRecords = table.Rows.Count;
            int numRecordsWritten = 0;

            // While we can batch
            while (remainingRecords > 0)
            {
                var rowsToInsertCount = Math.Min(remainingRecords, BATCH_SIZE);
                Log.Info(String.Format("[Batch insert] {0} rows into {1}, this+remaining:{2}", table.TableName, rowsToInsertCount, remainingRecords));
                lock (DbWriteLock)
                {
                    for (var i = 0; i < rowsToInsertCount; ++i)
                    {
                        Adapter.InsertRow(table.TableName, table.Rows[remainingRecords + i]);
                        numRecordsWritten++;
                    }
                    // decrement the remaining rows
                    remainingRecords -= rowsToInsertCount;
                }
                Log.Info(String.Format("[Batch insert] Done for {0} total remaining:{1} total written:{2}", table.TableName, remainingRecords, numRecordsWritten ));
            }

            Log.Info(String.Format("[Batch insert] done for: {0} - total inserted: {1}", table.TableName, numRecordsWritten));
            //// Write all rows in table.
            //int numRecordsWritten = 0;
            //foreach (DataRow row in table.Rows)
            //{
            //    try
            //    {
            //        lock (DbWriteLock)
            //        {
            //            Adapter.InsertRow(table.TableName, row);
            //            numRecordsWritten++;
            //        }
            //    }
            //    catch (DbException) { }
            //}
            Log.Debug(String.Format("Finished writing {0} {1}!", numRecordsWritten, "record".Pluralize(numRecordsWritten)));
        }

        public bool WaitForWriteFinish(int waitTimeout)
        {
            if (!Monitor.TryEnter(DbWriteLock, waitTimeout))
            {
                Log.Error("Could not acquire write lock; forcing exit..");
                return false;
            }

            Log.Debug("Acquired write lock gracefully..");
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Adapter.Dispose();
            }
            disposed = true;
        }
    }
}