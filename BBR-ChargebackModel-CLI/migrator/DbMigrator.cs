using System;
using Migrator;
using System.Reflection;

namespace BBR_ChargebackModel_CLI.migrator
{
    class DbMigrator
    {
        /// <summary>
        /// Migrate the database to its latest version.
        /// </summary>
        /// <param name="opts"></param>
        public static void MigrateToLatestDbVersion(string dbType, string connectionString)
        {
            var provider = ProviderFactory.Create(dbType, connectionString);
            var loader = new MigrationLoader(provider, Assembly.GetAssembly(typeof(DbMigrator)), false);
            var availableMigrations = loader.GetAvailableMigrations();


            CreateMigrator(dbType, connectionString).MigrateToLastVersion();
            //CreateMigrator(dbType, connectionString).MigrateTo(20151126132600);
        }

        /// <summary>
        /// Migrate the database back to its original form (without any migrations)
        /// </summary>
        /// <param name="opts"></param>
        public static void MigrateToEmptyDb(string dbType, string connectionString)
        {
            CreateMigrator(dbType, connectionString).MigrateTo(0);
        }

        /// <summary>
        /// Creates a new migrator from the options.
        /// </summary>
        /// <param name="opts"></param>
        /// <returns></returns>
        private static Migrator.Migrator CreateMigrator(string dbType, string connectionString)
        {
            var asm = Assembly.GetAssembly(typeof(migrations.M001_CreateLookupTable));
            Migrator.Migrator m = new Migrator.Migrator(dbType, connectionString, asm);
            return m;
        }
    }
}
