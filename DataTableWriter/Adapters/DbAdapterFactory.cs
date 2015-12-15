using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTableWriter.Connection;
using DataTableWriter.Drivers;

namespace DataTableWriter.Adapters
{
    public enum DbDriverType { Postgres, Oracle, MsSQL };

    public static class DbAdapterFactory
    {
        public static IDbAdapter GetInstance(DbDriverType driverType, IDbConnectionInfo dbConnectionInfo)
        {
            switch (driverType)
            {
                case DbDriverType.Postgres:
                    return new DbAdapter(new PostgresDriver(), dbConnectionInfo);

                case DbDriverType.Oracle:
                    return new DbAdapter(new OracleDriver(), dbConnectionInfo);

                case DbDriverType.MsSQL:
                    return new DbAdapter(new MsSQLDriver(), dbConnectionInfo);

                default:
                    throw new ArgumentException(String.Format("Invalid DB Driver Type '{0}' specified!", driverType));
            }
        }
    }
}
