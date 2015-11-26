using System;

namespace DataTableWriter.Drivers
{
    /// <summary>
    /// Enumeration of supported database driver types.
    /// </summary>
    public enum DbDriverType { Postgres, Oracle, MsSQL };

    /// <summary>
    /// Handles instantiation of DbDriver objects.
    /// </summary>
    internal static class DbDriverFactory
    {
        public static IDbDriver GetInstance(DbDriverType driverType)
        {
            switch (driverType)
            {
                case DbDriverType.Postgres:
                    return new PostgresDriver();

                case DbDriverType.Oracle:
                    return new OracleDriver();

                case DbDriverType.MsSQL:
                    return new MsSQLDriver();

                default:
                    throw new ArgumentException(String.Format("Invalid DB Driver Type '{0}' specified!", driverType));
            }
        }
    }
}