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
        /// <summary>
        /// Files that were successfully written to the database
        /// </summary>
        public IList<string> successfullyWrittenFiles = new List<string>();
        /// <summary>
        /// Files that failed with unrecoverable errors
        /// </summary>
        public IList<string> failedFiles = new List<string>();
        /// <summary>
        /// Files failed with unrecoverable configuration errors, but
        /// may be later resent when the configuration is corrected
        /// </summary>
        public IList<string> unsentFiles = new List<string>();

        /// <summary>
        /// Combines the contents of two output results
        /// </summary>
        /// <param name="parts"></param>
        public static OutputWriteResult Combine(params OutputWriteResult[] parts)
        {
            return parts.Aggregate(new OutputWriteResult(), (memo,part)=>{
                memo.failedFiles.AddRange(part.failedFiles);
                memo.successfullyWrittenFiles.AddRange(part.successfullyWrittenFiles);
                memo.unsentFiles.AddRange(part.unsentFiles);
                return memo;
            });
        }


        public static OutputWriteResult Aggregate<T>(IEnumerable<T> seq, Func<T, OutputWriteResult> fn )
        {
            return seq.Aggregate(new OutputWriteResult(), (memo, res) => Combine(memo, fn(res)));
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
