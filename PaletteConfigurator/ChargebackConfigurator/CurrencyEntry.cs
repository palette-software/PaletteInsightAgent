using System;
using System.Globalization;

namespace PaletteConfigurator.ChargebackConfigurator
{
    /// <summary>
    /// Helper class for using currencies in comboboxes
    /// </summary>
    public struct CurrencyEntry
    {
        public string Key;
        public string Name;
        public string EnglishName;

        public static CurrencyEntry Make(RegionInfo regionInfo)
        {
            return new CurrencyEntry
            {
                Key = regionInfo.ISOCurrencySymbol,
                Name = regionInfo.CurrencyNativeName,
                EnglishName = regionInfo.CurrencyEnglishName
            };
        }

        override public string ToString()
        {
            return String.Format("{2} ({1}) -- {0}", Name, Key, EnglishName);
        }
    }

}
