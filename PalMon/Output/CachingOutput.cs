using PalMon.LogPoller;
using PalMon.Sampler;
using PalMon.ThreadInfoPoller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    class CachingOutput
    {
        #region Internal stuff


        /// <summary>
        /// Helper map to figure out which cache to put a record into
        /// </summary>
        private Dictionary<string, DataTableCache> caches;

        private IOutput wrappedOutput;

        public CachingOutput(IOutput wrappedOutput)
        {
            this.wrappedOutput = wrappedOutput;

            caches = new Dictionary<string, DataTableCache>();

            AddCache(LogTables.makeFilterStateAuditTable());
            AddCache(LogTables.makeServerLogsTable());
            AddCache(ThreadTables.makeThreadInfoTable());
            AddCache(CounterSampler.makeCounterSamplesTable());

        }

        /// <summary>
        /// Adds a cache from a template DataTable
        /// </summary>
        /// <param name="table"></param>
        public void AddCache(DataTable table)
        {
            var tableName = table.TableName;
            var csvFileName = String.Format("csv/{0}", tableName);
            caches.Add(tableName, new DataTableCache(csvFileName, table, this.wrappedOutput.Write));
        }


        public bool HasCache(string tableName)
        {
            return caches.ContainsKey(tableName);
        }

        #endregion

        public void Tick()
        {
            // call Tick() on each cache
            foreach(var kv in caches)
            {
                kv.Value.Tick();
            }
        }

        public void Write(DataTable rows)
        {
            // check if we have the cache
            if (!caches.ContainsKey(rows.TableName))
                throw new ArgumentException(String.Format("Unknown table to write to:{0}", rows.TableName));

            // put the data there
            var cache = caches[rows.TableName];
            cache.Put(rows);
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose the output so it can close its connections if needed
                    wrappedOutput.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
