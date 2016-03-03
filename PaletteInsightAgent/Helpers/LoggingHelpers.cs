using NLog;
using System;

namespace PaletteInsightAgent.Helpers
{
    public class LoggingHelpers
    {

        public static void TimedLog(Logger Log, string message, Action act)
        {
            Log.Info("--> Starting {0}", message);
            var currentTime = DateTime.UtcNow;
            act();
            var endTime = DateTime.UtcNow;
            Log.Info("<-- Done [{1}ms] {0}", message,
                (endTime - currentTime).TotalMilliseconds);
        }


        /// <summary>
        /// A transparent generic timed logger that simply passes the
        /// return value of the delegate to its output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Log"></param>
        /// <param name="message"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static T TimedLog<T>(Logger Log, string message, Func<T> act)
        {
            Log.Info("--> Starting {0}", message);
            var currentTime = DateTime.UtcNow;
            var result = act();
            var endTime = DateTime.UtcNow;
            Log.Info("<-- Done [{1}ms] {0}", message,
                (endTime - currentTime).TotalMilliseconds);

            return result;
        }
    }
}
