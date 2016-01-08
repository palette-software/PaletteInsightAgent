using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.DbTesters
{
    public class DbTesterFactory
    {
        /// <summary>
        /// Return a new DbTester instance for the provided database type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static IDbTester CreateDbTester(string dbType)
        {
            switch (dbType)
            {
                case "Postgres": return new PostgresDbTester();
                default:
                    throw new ArgumentException(String.Format("Invalid Database type provided:{0}", dbType));
            }
        }
    }

}
