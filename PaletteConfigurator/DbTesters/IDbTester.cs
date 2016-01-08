using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.DbTesters
{

    public struct ConnectionTestResult
    {
        public bool success;
        public string message;
    }

    public interface IDbTester
    {
        string Name { get; }

        /// <summary>
        /// Returns thee connection string for a database
        /// </summary>
        /// <param name="dbDetails"></param>
        /// <returns></returns>
        string ConnectionString(DbDetails dbDetails);

        /// <summary>
        /// Returns true if the database specified by dbDetails
        /// is available.
        /// </summary>
        /// <param name="dbDetails"></param>
        /// <returns></returns>
        ConnectionTestResult VerifyConnection(DbDetails dbDetails);
    }
}
