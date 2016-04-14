using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
{

    /// <summary>
    /// An output wrapper for the results of an IOutput.Write().
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
        /// Combines the contents of two output results
        /// </summary>
        /// <param name="parts"></param>
        public static OutputWriteResult Combine(params OutputWriteResult[] parts)
        {
            return parts.Aggregate(new OutputWriteResult(), (memo,part)=>{
                memo.failedFiles.AddRange(part.failedFiles);
                memo.successfullyWrittenFiles.AddRange(part.successfullyWrittenFiles);
                return memo;
            });
        }


        public static OutputWriteResult Aggregate<T>(IEnumerable<T> seq, Func<T, OutputWriteResult> fn )
        {
            return seq.Aggregate(new OutputWriteResult(), (memo, res) => Combine(memo, fn(res)));
        }

        #region quickcreate
        public static OutputWriteResult Ok(params string[] files)
        {
            return new OutputWriteResult { successfullyWrittenFiles = new List<string>(files) };
        }

        public static OutputWriteResult Failed(params string[] files)
        {
            return new OutputWriteResult { failedFiles = new List<string>(files) };
        }

        #endregion
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
