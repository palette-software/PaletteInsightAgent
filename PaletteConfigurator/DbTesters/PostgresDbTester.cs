using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.DbTesters
{
    class PostgresDbTester : IDbTester
    {

        public string Name { get { return "Postgres"; } }

        public string ConnectionString(DbDetails dbDetails)
        {
            return String.Format("Host={0};Port={1};Username={2};Password={3};Database={4}",
                dbDetails.Host,
                dbDetails.Port,
                dbDetails.Username,
                dbDetails.Password,
                dbDetails.Database);
        }

        public ConnectionTestResult VerifyConnection(DbDetails dbDetails)
        {
            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString(dbDetails)))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;

                        // Insert some data
                        cmd.CommandText = "SELECT 'Hello world'";
                        var result = cmd.ExecuteScalar();

                        // if null, we failed
                        if (result == null) return new ConnectionTestResult { success = false };

                        // otherwise we might have succeeded
                        return new ConnectionTestResult
                        {
                            success = (string.Compare("Hello World", (string)result, true) == 0),
                        };

                    }
                }

            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return new ConnectionTestResult { success = false, message = e.Message };
            }
        }
    }
}
