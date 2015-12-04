using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller.Db
{

    /// <summary>
    /// Generic interface for DB query building.
    /// </summary>
    public interface IDbHelper
    {
        /// <summary>
        /// Connect to the database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IDbConnection ConnectTo(string connectionString);

        /// <summary>
        /// Create a new SQL command from an SQL statement.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        IDbCommand MakeSqlCommand(IDbConnection conn, string cmdText);

        /// <summary>
        /// Add a parameter to an SQL command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        void AddSqlParameter(IDbCommand cmd, string name, object val);
    }
}
