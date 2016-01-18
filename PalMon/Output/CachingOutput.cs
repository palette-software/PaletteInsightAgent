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
        /// The output we wrap with cache.
        /// </summary>
        private DataTableCache filterChangeCache;
        private DataTableCache serverLogsCache;


        /// <summary>
        /// Helper map to figure out which cache to put a record into
        /// </summary>
        private Dictionary<string, DataTableCache> caches;

        //private RowCache<FilterStateChangeRow> filterChangeQueue;
        //private RowCache<ServerLogRow> serverLogsQueue;
        private IOutput wrappedOutput;

        public CachingOutput(IOutput wrappedOutput)
        {
            this.wrappedOutput = wrappedOutput;
            //filterChangeQueue = new RowCache<FilterStateChangeRow>("csv/filter-state-audit", wrappedOutput.Write);
            //serverLogsQueue = new RowCache<ServerLogRow>("csv/serverlogs", wrappedOutput.Write);

            filterChangeCache = new DataTableCache("csv/filter-state-audit", LogPoller.LogTables.makeFilterStateAuditTable(), wrappedOutput.Write);
            serverLogsCache = new DataTableCache("csv/server-logs", LogPoller.LogTables.makeServerLogsTable(), wrappedOutput.Write);

            caches = new Dictionary<string, DataTableCache> {
                { filterChangeCache.TableName, filterChangeCache },
                { serverLogsCache.TableName, serverLogsCache }
            };
        }



        #endregion

        public void FlushIfNeeded()
        {
            //filterChangeQueue.FlushCacheIfNeeded();
            //serverLogsQueue.FlushCacheIfNeeded();

            filterChangeCache.FlushCacheIfNeeded();
            serverLogsCache.FlushCacheIfNeeded();
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


        public void Write(FilterStateChangeRow[] rows)
        {

            throw new NotImplementedException();
        }

        public void Write(ServerLogRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Write(string csvFile, ThreadInfoRow[] rows)
        {
            throw new NotImplementedException();
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
