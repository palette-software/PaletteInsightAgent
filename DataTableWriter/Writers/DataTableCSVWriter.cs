﻿using CsvHelper;
using log4net;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DataTableWriter.Writers
{
    /// <summary>
    /// Contains functionality for writing DataTable objects to CSV file.
    /// </summary>
    public class DataTableCsvWriter : IDataTableWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object CsvWriteLock = new object();
        private bool disposed;
        protected bool headerHasBeenWritten;
        protected StreamWriter streamWriter;
        protected CsvWriter csvWriter;

        public virtual string Name
        {
            get { return "CSV File Writer"; }
        }

        public DataTableCsvWriter(string filePath)
        {
            streamWriter = File.CreateText(filePath);
            csvWriter = new CsvWriter(streamWriter);
        }

        #region Public Methods

        /// <summary>
        /// Writes the data table to CSV.  Also ensures that the header has been written.
        /// </summary>
        /// <param name="table">The data table to write.</param>
        public void Write(DataTable table)
        {
            if (!headerHasBeenWritten)
            {
                lock (CsvWriteLock)
                {
                    Log.Debug("Writing header to CSV..");
                    WriteHeader(table);
                    headerHasBeenWritten = true;
                }
            }

            Log.Debug(String.Format("Writing {0} {1} to CSV..", table.Rows.Count, "record".Pluralize(table.Rows.Count)));
            foreach (DataRow row in table.Rows)
            {
                lock (CsvWriteLock)
                {
                    WriteRow(row);
                }
            }
            Log.Debug(String.Format("Finished writing {0}!", "record".Pluralize(table.Rows.Count)));
        }

        public bool WaitForWriteFinish(int waitTimeout)
        {
            if (!Monitor.TryEnter(CsvWriteLock, waitTimeout))
            {
                Log.Warn("Could not acquire write lock!");
                return false;
            }

            Log.Debug("Acquired write lock gracefully..");
            return true;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Writes header information to CSV.
        /// </summary>
        /// <param name="table">The table to use as a point of reference for header information.</param>
        protected void WriteHeader(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }
            csvWriter.NextRecord();
        }

        /// <summary>
        /// Writes a single row of data to CSV.
        /// </summary>
        /// <param name="row">The data row to write.</param>
        protected void WriteRow(DataRow row)
        {
            try
            {
                for (var i = 0; i < row.Table.Columns.Count; i++)
                {
                    if (row[i] != null)
                    {
                        csvWriter.WriteField(row[i]);
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                }
                csvWriter.NextRecord();
            }
            catch (CsvWriterException ex)
            {
                Log.Error("Error writing record to CSV: " + ex.Message);
            }
        }

        #endregion Protected Methods

        #region IDisposable Methods

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
                csvWriter.Dispose();
                streamWriter.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable Methods
    }
}