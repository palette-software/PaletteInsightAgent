using System.Text;

namespace PaletteInsightAgent.Helpers
{
    public class GreenplumCsvEscaper
    {

        /// <summary>
        /// Escapes a string to be GreenPlum CSV-compatible
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string EscapeField(string field)
        {
            var idx = 0;
            var len = field.Length;
            var output = new StringBuilder();
            // for each character
            while (idx < len)
            {
                // get the current char
                var ch = field[idx];

                switch (ch)
                {
                    // check if its a special character
                    case '\\': output.Append("\\\\"); break;
                    case ',': output.Append("\\,"); break;
                    case '\r': output.Append("\\r"); break;
                    case '\n': output.Append("\\n"); break;
                    case '\0': break;
                    // if not, simply append it
                    default: output.Append(ch); break;
                }

                // go to the next char
                idx++;
            }

            return output.ToString();
        }
    }
}
