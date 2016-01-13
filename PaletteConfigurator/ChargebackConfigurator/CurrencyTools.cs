using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.ChargebackConfigurator
{
    /// <summary>
    /// Helper class to get a list of currencies from .NET
    /// Cultures.
    /// </summary>
    public static class CurrencyTools
    {
        private static IDictionary<string, string> codeToSymbolMap;
        private static IDictionary<string, RegionInfo> nameToCodeMap;
        /// <summary>
        ///  Builds up a cache of currencies
        /// </summary>
        static CurrencyTools()
        {
            var regions = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.LCID);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol);

            codeToSymbolMap = regions
                .ToDictionary(x => x.Key, x => x.First().CurrencySymbol);

            nameToCodeMap = regions
                .ToDictionary(x => x.Key, x => x.First());
        }

        // Actually get a currency symbol
        public static bool TryGetCurrencySymbol(
                              string ISOCurrencySymbol,
                              out string symbol)
        {
            return codeToSymbolMap.TryGetValue(ISOCurrencySymbol, out symbol);
        }


        public static IDictionary<string, RegionInfo> NameToCodeMap
        {
            get { return nameToCodeMap; }
        }


        /// <summary>
        /// Returns a string representation of the regionInfo
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ToCurrencyString(RegionInfo x)
        {
            return String.Format("{1} ({0})", x.ISOCurrencySymbol, x.CurrencyEnglishName);
        }

    }
}
