using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    /// <summary>
    /// A generic interface for writing to the database.
    ///  NOTE: since we (possibly) need to close files and/or database connections,
    ///  instances of this interface has to extend IDisposable
    /// </summary>
    public interface IOutput : IDisposable
    {
        void Write(string csvFile, FilterStateChangeRow[] rows);
        void Write(string csvFile, ThreadInfoRow[] rows);
        void Write(string csvFile, ServerLogRow[] rows);
    }
}
