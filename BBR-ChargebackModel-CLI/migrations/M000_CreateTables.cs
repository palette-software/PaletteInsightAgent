using Migrator.Framework;
using System.Data;
using System;

namespace BBR_ChargebackModel_CLI.migrations
{
    [Migration(20151126104200)]
    public class M000_CreateTables : Migration
    {
        public override void Up()
        {
            //Database.ExecuteNonQuery("CREATE SCHEMA chargeback;");
            //CREATE TABLE chargeback_models(
            //    id serial PRIMARY KEY,
            //    created_at timestamp with time zone,
            //    effective_from timestamp with time zone,
            //    effective_to timestamp with time zone,

            //    timezone_for_chargeback varchar(255),
            //    unit_price_currency varchar(16)
            //);
            Database.AddTable("chargeback_models",
                new Column("id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                new Column("created_at", DbType.DateTime, 50),
                new Column("effective_from", DbType.DateTime, 100),
                new Column("effective_to", DbType.DateTime, 100),
                new Column("timezone_for_chargeback", DbType.String, 32),
                new Column("unit_price_currency", DbType.String, 16)
            );

            //CREATE TABLE chargeback_values(
            //    id serial PRIMARY KEY,
            //    usage_type integer,
            //    chargeback_model_id integer REFERENCES chargeback_models(id),
            //    day_of_week integer,
            //    hour_of_day integer,
            //    unit_price numeric,
            //    rate_category varchar(255)
            //);
            Database.AddTable("chargeback_values",
                new Column("id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                new Column("usage_type", DbType.Int32, 10),
                new Column("model_id", DbType.Int64, 50),
                new Column("day_of_week", DbType.Int32, 10),
                new Column("hour_of_day", DbType.Int32, 10),
                new Column("unit_price", DbType.Currency, 10),
                new Column("rate_category", DbType.String, 255)
            );
            Database.AddForeignKey("FK_chargeback_value_model", "chargeback_values", "model_id", "chargeback_models", "id");

            //CREATE TABLE chargeback_storage(
            //    id serial PRIMARY KEY,
            //    chargeback_model_id integer REFERENCES chargeback_models(id),
            //    unit_price numeric,
            //    unit_size_in_bytes integer
            //);
            Database.AddTable("chargeback_storage",
                new Column("id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                new Column("model_id", DbType.Int64),
                new Column("unit_price", DbType.Currency),
                new Column("unit_size_in_bytes", DbType.Int32)
            );
            Database.AddForeignKey("FK_chargeback_storage_model", "chargeback_storage", "model_id", "chargeback_models", "id");
        }

        public override void Down()
        {
            Database.RemoveTable("chargeback_storage");
            Database.RemoveTable("chargeback_values");
            Database.RemoveTable("chargeback_models");

            //Database.ExecuteNonQuery("DROP SCHEMA chargeback;");
        }
    }
}
