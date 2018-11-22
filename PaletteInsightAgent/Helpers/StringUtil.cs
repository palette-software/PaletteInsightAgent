using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Helpers
{
    class StringUtil
    {
        public static string ReplaceSubString(string logString, string pattern, int replaceCounter = 1)
        {
            string replacement = "";
            Regex rgx = new Regex(pattern);

            string result = rgx.Replace(logString, replacement, replaceCounter);
            return result;
        }
    }
}
