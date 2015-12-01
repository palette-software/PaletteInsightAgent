using System;
using System.Reflection;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabMon.LogPoller
{
    class ViewPathUpdaterAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string connectionString;
        private IViewPathUpdater viewPathUpdater;

        /// <summary>
        /// Creates a new instance of the view path updater
        /// </summary>
        /// <param name="connectionString"></param>
        public ViewPathUpdaterAgent(string connectionString)
        {
            this.connectionString = connectionString;

            viewPathUpdater = new PostgresViewPathUpdater(connectionString);
        }


        public void updateViewPath(ITableauRepoConn tableauRepo)
        {
            if (tableauRepo == null) return;
            Log.Info("Starting view path update....");
            viewPathUpdater.updateViewPaths(tableauRepo);
            Log.Info("Finished view path update....");
        }
    }
}
