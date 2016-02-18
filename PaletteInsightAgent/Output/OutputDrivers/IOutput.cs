﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
{

    /// <summary>
    /// An output wrapper for the results of an IOutput.Write()
    /// </summary>
    public class OutputWriteResult
    {
        public IList<string> successfullyWrittenFiles = new List<string>();
        public IList<string> failedFiles = new List<string>();

        /// <summary>
        /// Combines the contents of two output results
        /// </summary>
        /// <param name="parts"></param>
        public static OutputWriteResult Combine(params OutputWriteResult[] parts)
        {
            var o = new OutputWriteResult();

            foreach(var part in parts)
            {
                o.failedFiles.AddRange(part.failedFiles);
                o.successfullyWrittenFiles.AddRange(part.successfullyWrittenFiles);
            }

            return o;
        }
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
        OutputWriteResult Write(IList<string> csvFile);
    }
}
