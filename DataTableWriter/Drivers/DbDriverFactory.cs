using System;

namespace DataTableWriter.Drivers
{
    /// <summary>
    /// Enumeration of supported database driver types.
    /// </summary>
    public enum DbDriverType { Postgres, Oracle };

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

                default:
                    throw new ArgumentException(String.Format("Invalid DB Driver Type '{0}' specified!", driverType));
            }
        }
    }
}