using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Migrator.Framework;

namespace BBR_ChargebackModel_CLI.migrations
{
    [Migration(20151207114400)]
    public class M002_ChangeUsageTypeToString : Migration
    {
        public override void Up()
        {
            Database.RemoveColumn("chargeback_lookup", "usage_type");
            Database.RemoveColumn("chargeback_values", "usage_type");
            Database.AddColumn("chargeback_lookup", new Column("usage_type", System.Data.DbType.String, 32));
            Database.AddColumn("chargeback_values", new Column("usage_type", System.Data.DbType.String, 32));
        }

        public override void Down()
        {
            Database.RemoveColumn("chargeback_lookup", "usage_type");
            Database.RemoveColumn("chargeback_values", "usage_type");
            Database.AddColumn("chargeback_lookup", new Column("usage_type", System.Data.DbType.Int32));
            Database.AddColumn("chargeback_values", new Column("usage_type", System.Data.DbType.Int32));
        }
    }
}
