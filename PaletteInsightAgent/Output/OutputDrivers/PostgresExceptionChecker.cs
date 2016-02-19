using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output
{

    /// <summary>
    /// A class wrapping the exception handling policy of the Postgres Output
    /// </summary>
    class PostgresExceptionChecker
    {

        /// <summary>
        /// Checks if an exception stops a complete batch
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsFatalForBatch(Exception e)
        {
            // For now we know of no exceptions fatal for the whole batch
            // TODO: find the ones that are
            return false;
        }

        /// <summary>
        /// Checks if an exception can hinder the whole batch but can maybe re-uploaded later
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsTemporaryForBatch(Exception e)
        {
            return ExceptionIsTemporary(e);
        }

        /// <summary>
        /// Checks if an exception can hinder the whole batch but can-be re-tried on next launch
        /// (maybe after a config change)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsSessionFatalForBatch(Exception e)
        {
            return ExceptionIsSessionFatal(e);
        }


        /// <summary>
        /// Checks if the exception can be resolved by re-trying later
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsTemporaryForFile(Exception e)
        {
            return ExceptionIsTemporary(e);
        }

        /// <summary>
        /// Checks if the exception cannot be resolved by re-trying later.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsFatalForFile(Exception e)
        {
            if (e is NpgsqlException)
            {
                // except if the NpgSql exception message contains "invalid input syntax",
                // we can be pretty sure that this CSV file is not written in the way, we
                // could handle it. So there is no point in re-trying that file.
                if (e.Message.Contains("invalid input syntax")) return true;
                // When GP replies with "Line too long"
                if (e.Message.Contains("line too long")) return true;

                if (e.Message.Contains("does not contain")) return true;
            }
            // TODO: implement the rest
            return false;
        }

        /// <summary>
        /// Checks if an exception can hinder a file but can-be re-tried on next launch
        /// (maybe after a config change)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ExceptionIsSessionFatalForFile(Exception e)
        {
            return ExceptionIsSessionFatal(e);
        }

        #region Shared

        private static bool ExceptionIsTemporary(Exception e)
        {
            // For now we know of no exceptions fatal for the whole batch
            // TODO: network down may be one of these
            return false;
        }

        private static bool ExceptionIsSessionFatal(Exception e)
        {
            // on authentication failiure the exception is fatail for the session
            if (e is NpgsqlException && e.Message.Contains("authentication failed")) return true;

            return false;
        }


        #endregion

    }
}
