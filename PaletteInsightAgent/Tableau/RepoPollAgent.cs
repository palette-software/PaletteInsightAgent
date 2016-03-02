using NLog;
using PaletteInsightAgent.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.RepoTablesPoller
{
    class RepoPollAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string InProgressLock = "Repostiory Tables";

        public void Poll(ITableauRepoConn connection, ICollection<string> tableNames)
        {
            Log.Info("Polling Tableau repository tables.");
            foreach (var tableName in tableNames)
            {
                DataTable table = connection.GetTable(tableName);
                OutputSerializer.Write(table);
            }
        }
    }
}
