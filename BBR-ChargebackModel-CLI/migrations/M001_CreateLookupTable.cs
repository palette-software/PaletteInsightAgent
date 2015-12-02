using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Migrator.Framework;

namespace BBR_ChargebackModel_CLI.migrations
{
    [Migration(20151126132600)]
    public class M001_CreateLookupTable : Migration
    {
        public override void Up()
        {
            Database.AddTable("chargeback_lookup",
                //new Column("id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),

                new Column("datetime_key", DbType.DateTime, ColumnProperty.PrimaryKey),

                new Column("model_id", DbType.Int64),
                new Column("usage_type", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("unit_price", DbType.Currency),
                new Column("rate_category", DbType.String, 32),

                new Column("unit_price_currency", DbType.String, 16)
            );

            Database.AddForeignKey("FK_chargeback_lookup_model", "chargeback_lookup", "model_id", "chargeback_models", "id");
        }

        public override void Down()
        {
            Database.RemoveTable("chargeback_lookup");
        }
    }
}
