using System;
using System.Data;

namespace DataTableWriter.Writers
{
    /// <summary>
    /// Interface describing an object capable of writing out DataTable objects.
    /// </summary>
    public interface IDataTableWriter : IDisposable
    {
        string Name { get; }

        void Write(DataTable table);

        /// <summary>
        /// Waits for writer to finish its ongoing write operations for the
        /// specified amount of time.
        /// </summary>
        /// <param name="waitTimeout">Timeframe given in millisecs</param>
        /// <returns>If the write operations do not finish within the given
        /// timeframe, it returns false. Otherwise it returns true.</returns>
        bool WaitForWriteFinish(int waitTimeout);
    }
}
