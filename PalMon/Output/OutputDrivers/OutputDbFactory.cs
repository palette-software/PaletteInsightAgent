using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{
    public class OutputDbFactory
    {
        /// <summary>
        /// Factory method to create a DB output
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="resultDb"></param>
        /// <returns></returns>
        public static IOutput DriverFor(string databaseType, IDbConnectionInfo resultDb)
        {
            switch(databaseType)
            {
                //case "Oracle": return new OracleOutput(resultDb);
                case "Postgres": return new PostgresOutput(resultDb);
            }
            throw new ArgumentException(String.Format("Unknown database type: {0}", databaseType));
        }
    }
}
