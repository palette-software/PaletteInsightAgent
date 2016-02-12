using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{

    /// <summary>
    /// An output wrapper for the results of an IOutput.Write()
    /// </summary>
    public struct IOutputWriteResult
    {
        public IList<string> successfullyWrittenFiles;
        public IList<string> failedFiles;
    }

    /// <summary>
    /// A generic interface for writing to the database.
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
        IOutputWriteResult Write(IList<string> csvFile);
    }
}
