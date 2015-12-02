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
                            unit_price_currency,
                            storage_unit_price,
                            storage_bytes_per_unit
                        )
                        VALUES (@created_at, @effective_from, @effective_to, @timezone_for_chargeback, @unit_price_currency, @storage_unit_price, @storage_bytes_per_unit)
                        RETURNING id";

                AddParamToQuery(cmd, "created_at", DateTime.UtcNow);
                AddParamToQuery(cmd, "effective_from", model.EffectiveFrom);
                AddParamToQuery(cmd, "effective_to", model.EffectiveTo);
                AddParamToQuery(cmd, "timezone_for_chargeback", model.TimezoneForChargeback);
                AddParamToQuery(cmd, "unit_price_currency", model.UnitPriceCurrency);

                AddParamToQuery(cmd, "storage_unit_price", model.StorageUnitPrice);
                AddParamToQuery(cmd, "storage_bytes_per_unit", model.StoregeBytesPerUnit);
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

        public static void InsertLookupEntries(long modelId, NpgsqlConnection conn, LookupRow[] rows)
        {
            Console.WriteLine(String.Format("Creating chargeback lookup data - {0} rows", rows.Length));
            foreach (var row in rows)
            {
                // Remove any existing row for this time
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"DELETE FROM chargeback_lookup WHERE datetime_key=@datetime_key;";
                    AddParamToQuery(cmd, "datetime_key", row.DatetimeKey);
                    cmd.ExecuteNonQuery();
                }

                // Add the new row
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
