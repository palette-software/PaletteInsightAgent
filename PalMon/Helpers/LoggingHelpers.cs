using NLog;
using System;

namespace PalMon.Helpers
{
    public class LoggingHelpers
    {

        public static void TimedLog(Logger Log, string message, Func<int> act)
        {
            Log.Info("--> Starting {0}", message);
            var currentTime = DateTime.UtcNow;
            int rowsWritten = act();
            var endTime = DateTime.UtcNow;
            Log.Info("<-- Done [{1}ms] {0}. Total rows written: {2}", message,
                (endTime - currentTime).TotalMilliseconds, rowsWritten);
        }
    }
}
