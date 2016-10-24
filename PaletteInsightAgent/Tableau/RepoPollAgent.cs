using NLog;
using PaletteInsight.Configuration;
using PaletteInsightAgent.Helpers;
using PaletteInsightAgent.Output;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
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
            tables.Where(table => !table.Full)
                .ToList()
                .ForEach((table) =>
                {
                    var tableName = table.Name;

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
                        var maxIdPromise = APIClient.GetMaxId(tableName);
                        maxIdPromise.Wait();
                        var maxId = maxIdPromise.Result;

                        // Get data from that max id
                        string newMax;
                        DataTable dataTable = connection.GetStreamingTable(tableName, table, maxId, out newMax);
                        if (dataTable != null)
                        {
                            OutputSerializer.Write(dataTable, newMax);
                        }
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle((x) =>
                        {
                            if (x is HttpRequestException || x is TaskCanceledException)
                            {
                                // HttpRequestException is expected on network errors. TaskCanceledException is thrown if the async task (HTTP request) timed out.
                                Log.Warn(x, "Polling streaming table: '{0}' timed out! Exception: ", tableName);
                            }
                            else if (x is TemporaryException)
                            {
                                // It is already a TemporaryException, just pass it on to the handler.
                                throw x;
                            }

                            Log.Error(x, "Async exception caught while polling streaming table: {0}! Exception: ", tableName);
                            return true;
                        });
                    }
                    catch (TemporaryException tex)
                    {
                        Log.Warn("Temporarily unable to get max ID for table: {0}! Exception: {1}", tableName, tex);
                    }
                    catch (TaskCanceledException tce)
                    {
                        // This should be only a temporary condition, it is only a problem if it occurs many times in a row.
                        Log.Warn("Polling streaming table: '{0}' timed out! Exception: {1}", tableName, tce);
                    }
                    catch (HttpRequestException hre)
                    {
                        Log.Warn("HTTP request exception while polling streaming table: '{0}'! Exception: {1}", tableName, hre);
                    }
                    catch (Exception e)
                    {
                        if (e is InvalidOperationException && e.Message.Contains("Connection property has not been initialized"))
                        {
                            // This might also mean that the connection to Tableau is down
                            Log.Warn(e, "Temporarily unable to poll streaming table: '{0}'! Exception: ", tableName);
                            return;
                        }
                        Log.Error(e, "Error while polling streaming table: '{0}'! Exception: ", tableName);
                    }
                });
        }
    }
}
