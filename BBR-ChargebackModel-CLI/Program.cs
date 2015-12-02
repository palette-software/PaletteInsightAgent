using System;
using System.Data;
using Npgsql;
using FileHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Migrator;
using Migrator.Framework;


namespace BBR_ChargebackModel_CLI
{
    class CLIConfig
    {
        public string DbType { get; set; }
        public string ConnectionString { get; set; }

        public string ModelFile { get; set; }
        public string ValuesFile { get; set; }

        public bool Revert { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var opts = GetCLIOptions(args);

            // If we need to re-migrate, rever the db to its original state
            if (opts.Revert)
            {
                migrator.DbMigrator.MigrateToEmptyDb(opts.DbType, opts.ConnectionString);
            }

            // Migrate the DB to the current version
            migrator.DbMigrator.MigrateToLatestDbVersion(opts.DbType, opts.ConnectionString);

            // Then import the data
            try
            {
                importData(opts.DbType, opts.ConnectionString,
                    ChargebackModel.FromFile(opts.ModelFile),
                    ChargebackValue.FromFile(opts.ValuesFile));
            }
            catch (NoChargebackConfigured e)
            {
                Console.Error.WriteLine("Cannot fill chargeback lookup table: at least a day/hour/usage-type combination is left blank:");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                // Signal that things arent OK
                Environment.Exit(-1);
            }
        }
        /// <summary>
        /// Exits the program if the command line options arent OK, returns the pre-set options otherwise
        /// </summary>
        /// <param name="args">The list of command line args.</param>
        private static CLIConfig GetCLIOptions(string[] args)
        {
            // we may have invalid number of arguments
            if (args.Length < 4 || args.Length > 5)
            {
                DisplayUsageInformationAndExit();
            }

            // we may not want to revert
            if (args.Length == 4)
            {
                return new CLIConfig { DbType = args[0], ConnectionString = args[1], ModelFile = args[2], ValuesFile = args[3], Revert = false };
            }

            // or we may want to revert
            if (args.Length == 5)
            {
                if (args[0] != "revert") DisplayUsageInformationAndExit();
                return new CLIConfig { DbType = args[1], ConnectionString = args[2], ModelFile = args[3], ValuesFile = args[4], Revert = true };
            }

            return null;

        }

        private static void DisplayUsageInformationAndExit()
        {
            Console.Out.WriteLine("Usage: BBR-ChargebackModel-CLI <revert|> <dbtype> <connection_string> <model_file> <values_file>\n\n");
            Console.Out.WriteLine("  revert            : When this switch is set, the tables are dropped and re-migrated before run.");
            Console.Out.WriteLine("  dbtype            : the database type. Can be 'Postgres' or 'SqlServer' or 'Oracle'");
            Console.Out.WriteLine("  connection_string : The C# ADO connection string for the database");
            Console.Out.WriteLine("  model_file        : The CSV file containing the chargeback model.");
            Console.Out.WriteLine("  values_file       : The CSV file containing the hour-by-hour chargeback values.");

            Environment.Exit(-1);
        }


        /// <summary>
        /// Dispatch function for the importing of the data based on the DB
        /// </summary>
        /// <param name="dbType"></param>
        private static void importData(string dbType, string connectionString, ChargebackModel model, ChargebackValue[] values)
        {

            switch(dbType)
            {
                case "Postgres":
                    var chargebackLookup = ChargebackLookupCreator.CreateChargebackLookup(model, values);
                    var modelId = db.PostgresDb.importDataPostgres(connectionString, model, values, chargebackLookup );
                    break;
                default:
                    Console.Error.WriteLine("Invalid db type: " + dbType);
                    Environment.Exit(-1);
                    break;
            }

        }


    }
}
