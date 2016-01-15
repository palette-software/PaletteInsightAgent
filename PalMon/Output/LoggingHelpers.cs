using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    public class LoggingHelpers
    {
        public static void TimedLog(string message, Action act)
        {
            Console.Out.WriteLine(String.Format("--> Starting {0}", message));
            var currentTime = DateTime.UtcNow;
            act();
            var endTime = DateTime.UtcNow;
            Console.Out.WriteLine(String.Format("<-- Done [{1}ms] {0}", message,
                (endTime - currentTime).TotalMilliseconds));
        }
    }
}
