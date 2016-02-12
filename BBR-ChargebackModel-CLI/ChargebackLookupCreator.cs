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
        public NoChargebackConfigured(int dow, int hod, int usageType)
            : base(BuildMessage(dow, hod, usageType))
        {
        }

        private static string BuildMessage(int dow, int hod, int usageType)
        {
            return String.Format("Cannot find chargeback setup for day_of_the_week={0}, hour_of_day={1}, usage_type={2}", dow, hod, usageType);
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
        private const int USAGE_TYPE_COUNT = 2;
        /// <summary>
        /// Creates the chargeback lookup table.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="values"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public static LookupRow[] CreateChargebackLookup(ChargebackModel model, ChargebackValue[] values)
        {
            LookupEntry[,,] chargebackLookup = CreateLookupEntries(values);

            // Figure out the timezone used
            var timezoneUsed = TimeZoneInfo.FindSystemTimeZoneById(model.TimezoneForChargeback);

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

                // Handle all usage types
                for (var usageType = 0; usageType < USAGE_TYPE_COUNT; ++usageType)
                {
                    var lookup = chargebackLookup[dow, hod, usageType];

                    // Set up the effective range
                    try
                    {
                        var effectiveFrom = TimeZoneInfo.ConvertTimeToUtc(currentDate, timezoneUsed);

                        outputList.Add(new LookupRow
                        {
                            DatetimeKey = effectiveFrom,
                            ModelId = 0,
                            UsageType = lookup.UsageType,
                            UnitPrice = lookup.UnitPrice,
                            RateCategory = lookup.RateCategory,
                            UnitPriceCurrency = model.UnitPriceCurrency
                        });
                    }
                    catch(Exception e)
                    {
                        // skip this field if the time we want to convert is a non-existant one (daylight-savings-skipped)
                        Console.Error.WriteLine(e.ToString());
                    }
                }

                // advance to the next hour
                currentDate = currentDate.AddHours(1);
            }

            return outputList.ToArray();
        }

        private static void ValidateLookupEntries(LookupEntry[,,] chargebackLookup)
        {
            // Check if all slots are filled
            for (var i = 0; i < 7; ++i)
                for (var j = 0; j < 24; ++j)
                    for (var usageType = 0; usageType < USAGE_TYPE_COUNT; ++usageType)
                        // This check disallows defining LookupEntry as a struct.
                        if (chargebackLookup[i, j, usageType] == null)
                            throw new NoChargebackConfigured(i, j, usageType);
        }

        private static LookupEntry[,,] CreateLookupEntries(ChargebackValue[] values)
        {
            // Create a 3 dimensional lookup array: day-of-the-week, hour-of-day, usage-type
            var chargebackLookup = new LookupEntry[7, 24, USAGE_TYPE_COUNT];
            foreach (var val in values)
            {
                chargebackLookup[val.DayOfWeek, val.HourOfDay, (int)val.UsageType] = new LookupEntry
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
