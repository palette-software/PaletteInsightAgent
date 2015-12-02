using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller.Db
{
    class DbHelperFactory
    {
        public static IDbHelper MakeDbHelper(string dbType)
        {
            switch (dbType)
            {
                case "Postgres":
                    return new PostgresDbHelper();
            }

            throw new ArgumentException(String.Format("Unknown database type for IDbHelper: '{0}'", dbType ));
        }
    }
}
