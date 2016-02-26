using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Licensing
{
    /// <summary>
    /// Helper class for dealing with text-based byte stuff
    /// </summary>
    public class LicenseFormatting
    {
        /// <summary>
        /// Converts a byte array to a license string suitable for sending over via
        /// email, copy-pasting it, etc
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string toWrappedString(byte[] bytes, int wrapWidth=60)
        {
            return toWrapped(Convert.ToBase64String(bytes), wrapWidth);
        }

        public static byte[] fromWrappedString(string data)
        {
            return Convert.FromBase64String(fromWrapped(data));
        }

        #region width-wrapping


        public static string toWrapped(string source, int width = 60)
        {
            return source
                .ToList<char>()
                .Partition(width)
                .Select((line) => new String(line.ToArray()))
                .Join("\n");
        }

        const string WRAP_SPLIT_PATTERN = @"\s+";

        public static string fromWrapped(string source)
        {
            return Regex.Split(source, WRAP_SPLIT_PATTERN).Join("");
        }

        #endregion
    }
}
