using System.Diagnostics;

namespace PaletteGUI
{
    class HttpHelpers
    {
        /// <summary>
        /// Opens a web page in the default browser
        /// </summary>
        /// <param name="url"></param>
        public static void openWebPage(string url)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(url);
            Process.Start(sInfo);
        }
    }
}
