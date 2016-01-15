using System;
using System.Collections.Generic;
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
        private RowCache<FilterStateChangeRow> filterChangeQueue;
        private RowCache<ServerLogRow> serverLogsQueue;

        public CachingOutput(IOutput wrappedOutput)
        {
            filterChangeQueue = new RowCache<FilterStateChangeRow>("csv/filter-state-audit", wrappedOutput.Write);
            serverLogsQueue = new RowCache<ServerLogRow>("csv/serverlogs", wrappedOutput.Write);
        }



        #endregion

        public void Write(FilterStateChangeRow[] rows)
        {
            filterChangeQueue.Put(rows);
        }

        public void Write(ServerLogRow[] rows)
        {
            serverLogsQueue.Put(rows);
        }

        //public void Write(string csvFile, ThreadInfoRow[] rows)
        //{
        //    throw new NotImplementedException();
        //}


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
