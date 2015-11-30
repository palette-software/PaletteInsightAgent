using System;
using Npgsql;

namespace BBR_ChargebackModel_CLI.db
{
    class PostgresDb
    {


        public static Int64 importDataPostgres(string connectionString, ChargebackModel model, ChargebackValue[] values, LookupRow[] lookupRows)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var modelId = InsertChargebackModel(model, conn);
                InsertChargebackValues(values, modelId, conn);
                InsertLookupEntries(modelId, conn, lookupRows);

                //CreateChargebackLookup(model, values, modelId, conn);
                return modelId;

            }
        }


        /// <summary>
        /// Add the chargeback model to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        private static Int64 InsertChargebackModel(ChargebackModel model, NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = @"INSERT INTO chargeback_models(
                            created_at, 
                            effective_from,
                            effective_to,
                            timezone_for_chargeback,
                            unit_price_currency
                        )
                        VALUES (@created_at, @effective_from, @effective_to, @timezone_for_chargeback, @unit_price_currency)
                        RETURNING id";

                AddParamToQuery(cmd, "created_at", DateTime.UtcNow);
                AddParamToQuery(cmd, "effective_from", model.EffectiveFrom);
                AddParamToQuery(cmd, "effective_to", model.EffectiveTo);
                AddParamToQuery(cmd, "timezone_for_chargeback", model.TimezoneForChargeback);
                AddParamToQuery(cmd, "unit_price_currency", model.UnitPriceCurrency);

                // Run it!
                //cmd.ExecuteNonQuery();
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Add the chargeback model to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conn"></param>
        private static void InsertChargebackValues(ChargebackValue[] values, Int64 modelId, NpgsqlConnection conn)
        {
            foreach (var value in values)
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"INSERT INTO chargeback_values( usage_type, model_id, day_of_week, hour_of_day, unit_price, rate_category )
                        VALUES ( @usage_type, @model_id, @day_of_week, @hour_of_day, @unit_price, @rate_category )";

                    AddParamToQuery(cmd, "usage_type", (int)value.UsageType);
                    AddParamToQuery(cmd, "model_id", modelId);
                    AddParamToQuery(cmd, "day_of_week", value.DayOfWeek);
                    AddParamToQuery(cmd, "hour_of_day", value.HourOfDay);
                    AddParamToQuery(cmd, "unit_price", value.UnitPrice);
                    AddParamToQuery(cmd, "rate_category", value.RateCategory);


                    // Run it!
                    cmd.ExecuteNonQuery();
                }

            }
        }

        //private class LookupEntry
        //{
        //    public ChargebackUsageType UsageType;
        //    public decimal UnitPrice;
        //    public string RateCategory;
        //}

        //public class NoChargebackConfigured : Exception
        //{
        //    public NoChargebackConfigured(int dow, int hod)
        //    {
        //        DayOfWeek = dow;
        //        HourOfDay = hod;
        //    }

        //    public int DayOfWeek;
        //    public int HourOfDay;

        //    public override string ToString()
        //    {
        //        return String.Format("Cannot find chargeback setup for day_of_the_week={0}, hour_of_day={1}", DayOfWeek, HourOfDay);
        //    }
        //}

        //private static void CreateChargebackLookup(ChargebackModel model, ChargebackValue[] values, long modelId, NpgsqlConnection conn)
        //{
        //    // Create a 2 dimensional lookup array.
        //    var chargebackLookup = new LookupEntry[7,24];
        //    foreach (var val in values)
        //    {
        //        chargebackLookup[val.DayOfWeek, val.HourOfDay] = new LookupEntry
        //        {
        //            UsageType = val.UsageType,
        //            UnitPrice = val.UnitPrice,
        //            RateCategory = val.RateCategory
        //        };
        //    }

        //    // Check if all slots are filled
        //    for (var i = 0; i < 7; ++i)
        //        for (var j = 0; j < 24; ++j)
        //            if (chargebackLookup[i, j] == null)
        //                throw new NoChargebackConfigured(i, j);


        //    // Figure out the timezone used
        //    var tz = TimeZoneInfo.FindSystemTimeZoneById(model.TimezoneForChargeback);

        //    var startDate = model.EffectiveFrom;
        //    var endDate = model.EffectiveTo;
        //    // Start on top of the hour
        //    var currentDate = new DateTime( startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0 );

        //    while(currentDate < endDate)
        //    {
        //        var dow = (int)currentDate.DayOfWeek;
        //        var hod = currentDate.Hour;

        //        var lookup = chargebackLookup[dow, hod];

        //        // Double-check....
        //        // TODO: remove this if sure
        //        if (lookup == null)
        //            throw new NoChargebackConfigured(dow, hod);

        //        // Set up the effective range
        //        var effectiveFrom = TimeZoneInfo.ConvertTimeToUtc(currentDate, tz);

        //        InsertLookupRow(model, modelId, conn, lookup, effectiveFrom);
        //        // advance to the next hour
        //        currentDate = currentDate.AddHours(1);
        //    }

        //}

        //private static void InsertLookupRow(ChargebackModel model, long modelId, NpgsqlConnection conn, LookupEntry lookup, DateTime effectiveFrom)
        //{
        //    using (var cmd = new NpgsqlCommand())
        //    {
        //        cmd.Connection = conn;
        //        cmd.CommandType = System.Data.CommandType.Text;
        //        cmd.CommandText = @"INSERT INTO chargeback_lookup( datetime_key, model_id, usage_type, unit_price, rate_category, unit_price_currency )
        //                VALUES ( @datetime_key, @model_id, @usage_type, @unit_price, @rate_category, @unit_price_currency)";

        //        AddParamToQuery(cmd, "datetime_key", effectiveFrom);
        //        AddParamToQuery(cmd, "model_id", modelId);
        //        AddParamToQuery(cmd, "usage_type", (int)lookup.UsageType);
        //        AddParamToQuery(cmd, "unit_price", lookup.UnitPrice);
        //        AddParamToQuery(cmd, "rate_category", lookup.RateCategory);
        //        AddParamToQuery(cmd, "unit_price_currency", model.UnitPriceCurrency);


        //        // Run it!
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        public static void InsertLookupEntries(long modelId, NpgsqlConnection conn, LookupRow[] rows)
        {
            foreach(var row in rows)
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"INSERT INTO chargeback_lookup( datetime_key, model_id, usage_type, unit_price, rate_category, unit_price_currency )
                        VALUES ( @datetime_key, @model_id, @usage_type, @unit_price, @rate_category, @unit_price_currency)";

                    AddParamToQuery(cmd, "datetime_key", row.DatetimeKey);
                    AddParamToQuery(cmd, "model_id", modelId);
                    AddParamToQuery(cmd, "usage_type", (int)row.UsageType);
                    AddParamToQuery(cmd, "unit_price", row.UnitPrice);
                    AddParamToQuery(cmd, "rate_category", row.RateCategory);
                    AddParamToQuery(cmd, "unit_price_currency", row.UnitPriceCurrency);


                    // Run it!
                    cmd.ExecuteNonQuery();
                }

            }
        }


        private static void AddParamToQuery(NpgsqlCommand cmd, string paramName, object paramValue)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@" + paramName;
            parameter.Value = paramValue;
            cmd.Parameters.Add(parameter);
        }
    }
}
