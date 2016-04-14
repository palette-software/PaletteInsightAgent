using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
{

    /// <summary>
    /// A generic interface for writing data
    ///  NOTE: since we (possibly) need to close files and/or database connections,
    ///  instances of this interface has to extend IDisposable
    /// </summary>
    public interface IOutput : IDisposable
    {
        /// <summary>
        /// Tries to write out the csv files, and returns the list of successfully uploaded
        /// files.
        /// </summary>
        /// <param name="csvFile"></param>
        /// <returns></returns>
        void Write(string csvFile);

        /// <summary>
        /// Returns true if an upload is in progress for the given table.
        /// files.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        bool IsInProgress(string tableName);
    }
}
