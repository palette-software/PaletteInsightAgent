using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBR_ChargebackModel_CLI
{
    public class NoChargebackConfigured : Exception
    {
        public NoChargebackConfigured(int dow, int hod)
        {
            DayOfWeek = dow;
            HourOfDay = hod;
        }

        public int DayOfWeek;
        public int HourOfDay;

        public override string ToString()
        {
            return String.Format("Cannot find chargeback setup for day_of_the_week={0}, hour_of_day={1}", DayOfWeek, HourOfDay);
        }
    }

    /// <summary>
    /// Output structure for the lookup rows
    /// </summary>
    public struct LookupRow
    {
        public DateTime DatetimeKey;
        public Int64 ModelId;
        public ChargebackUsageType UsageType;
        public decimal UnitPrice;
        public string RateCategory;
        public string UnitPriceCurrency;

    }

    class ChargebackLookupCreator
    {
        /// <summary>
        /// Creates the chargeback lookup table.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="values"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public static LookupRow[] CreateChargebackLookup(ChargebackModel model, ChargebackValue[] values)
        {
            LookupEntry[,] chargebackLookup = CreateLookupEntries(values);

            // Figure out the timezone used
            var tz = TimeZoneInfo.FindSystemTimeZoneById(model.TimezoneForChargeback);

            var startDate = model.EffectiveFrom;
            var endDate = model.EffectiveTo;
            // Start on top of the hour
            var currentDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);

            // Create the output storage
            var outputList = new List<LookupRow>();

            while (currentDate < endDate)
            {
                var dow = (int)currentDate.DayOfWeek;
                var hod = currentDate.Hour;

                var lookup = chargebackLookup[dow, hod];

                // Double-check....
                // TODO: remove this if sure
                if (lookup == null)
                    throw new NoChargebackConfigured(dow, hod);

                // Set up the effective range
                var effectiveFrom = TimeZoneInfo.ConvertTimeToUtc(currentDate, tz);

                outputList.Add(new LookupRow
                {
                    DatetimeKey = effectiveFrom,
                    ModelId = 0,
                    UsageType = lookup.UsageType,
                    UnitPrice = lookup.UnitPrice,
                    RateCategory = lookup.RateCategory,
                    UnitPriceCurrency = model.UnitPriceCurrency
                });

                // advance to the next hour
                currentDate = currentDate.AddHours(1);
            }

            return outputList.ToArray();
        }

        private static void ValidateLookupEntries(LookupEntry[,] chargebackLookup)
        {
            // Check if all slots are filled
            for (var i = 0; i < 7; ++i)
                for (var j = 0; j < 24; ++j)
                    // This check disallows defining LookupEntry as a struct.
                    if (chargebackLookup[i, j] == null)
                        throw new NoChargebackConfigured(i, j);
        }

        private static LookupEntry[,] CreateLookupEntries(ChargebackValue[] values)
        {
            // Create a 2 dimensional lookup array.
            var chargebackLookup = new LookupEntry[7, 24];
            foreach (var val in values)
            {
                chargebackLookup[val.DayOfWeek, val.HourOfDay] = new LookupEntry
                {
                    UsageType = val.UsageType,
                    UnitPrice = val.UnitPrice,
                    RateCategory = val.RateCategory
                };
            }
            ValidateLookupEntries(chargebackLookup);

            return chargebackLookup;
        }

        private class LookupEntry
        {
            public ChargebackUsageType UsageType;
            public decimal UnitPrice;
            public string RateCategory;
        }

    }
}
