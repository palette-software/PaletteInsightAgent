using NLog;
using PaletteInsight.Configuration;
using PaletteInsightAgent.Helpers;
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
        public static readonly string FullTablesInProgressLock = "Repository Tables";
        public static readonly string StreamingTablesInProgressLock = "Streaming Tables";
        private static IDictionary<string, string> lastMaxId = new Dictionary<string, string>();

        public void PollFullTables(ITableauRepoConn connection, ICollection<RepoTable> tables)
        {
            Log.Info("Polling Tableau repository tables.");
            tables.Where(t => t.Full)
                .Select(t => t.Name)
                .ToList()
                .ForEach(t =>
                {
                    DataTable table = connection.GetTable(t);
                    OutputSerializer.Write(table);
                });
        }

        public void PollStreamingTables(ITableauRepoConn connection, ICollection<RepoTable> tables, IOutput output)
        {
            Log.Info("Polling Tableau Streaming tables.");
            tables.Where(t => !t.Full)
                .ToList()
                .ForEach(async (t) =>
                {
                    var tableName = t.Name;

                    try
                    {
                        // Delete all pending files for that streaming table
                        OutputSerializer.Delete(tableName);

                        // If we have a pending request for this table, then just skip this iteration
                        if (output.IsInProgress(tableName))
                        {
                            return;
                        }

                        // Ask web service what is the max id
                        var maxId = await APIClient.GetMaxId(tableName);

                        // Get data from that max id
                        string newMax;
                        DataTable table = connection.GetStreamingTable(tableName, t.Field, t.Filter, maxId, out newMax);
                        if (table != null)
                        {
                            OutputSerializer.Write(table, newMax);
                        }
                    }
                    catch (TemporaryException tex)
                    {
                        Log.Warn("Temporarily unable to get max ID for table: {0}! Exception: {1}", tableName, tex);
                    }
                    catch (TaskCanceledException tce)
                    {
                        // This should be only a temporary condition, it is only a problem if it occurs many times in a row.
                        Log.Warn("Polling streaming tables timed out! Exception: {0}", tce);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error while polling streaming table! Exception: ");
                    }
                });

        }
    }
}
