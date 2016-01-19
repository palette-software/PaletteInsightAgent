using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Helpers
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
    }
}
