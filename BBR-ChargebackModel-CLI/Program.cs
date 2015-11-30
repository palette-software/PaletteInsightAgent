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
    }

    class Program
    {
        static void Main(string[] args)
        {
            var opts = GetCLIOptions(args);

            migrator.DbMigrator.MigrateToEmptyDb(opts.DbType, opts.ConnectionString);
            migrator.DbMigrator.MigrateToLatestDbVersion(opts.DbType, opts.ConnectionString);


            var modelFile = @"c:\Users\Miles\Documents\Visual Studio 2015\Projects\BBR-ChargebackModel-CLI\BBR-ChargebackModel-CLI\chargeback_model.csv";

            var valuesFile = @"c:\Users\Miles\Documents\Visual Studio 2015\Projects\BBR-ChargebackModel-CLI\BBR-ChargebackModel-CLI\chargeback_values.csv";

            importData(opts.DbType, opts.ConnectionString,
                ChargebackModel.FromFile(modelFile),
                ChargebackValue.FromFile(valuesFile));

            Console.ReadLine();
        }
        /// <summary>
        /// Exits the program if the command line options arent OK, returns the pre-set options otherwise
        /// </summary>
        /// <param name="args">The list of command line args.</param>
        private static CLIConfig GetCLIOptions(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Out.WriteLine("Usage: BBR-ChargebackModel-CLI <dbtype> <connection_string> <file>");
                Console.Out.WriteLine("\n  dbtype : the database type. Can be 'Postgres' or 'SqlServer' or 'Oracle'");

                Environment.Exit(-1);
            }

            return new CLIConfig { DbType = args[0], ConnectionString = args[1]};
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
