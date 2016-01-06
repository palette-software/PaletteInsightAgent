using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return LicenseSerializer.toWrapped(Convert.ToBase64String(bytes), wrapWidth);
        }
    }
}
