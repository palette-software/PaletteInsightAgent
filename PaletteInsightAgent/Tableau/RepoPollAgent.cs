using NLog;
using PaletteInsightAgent.Configuration;
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
        private static IDictionary<string, string> localMaxId = new Dictionary<string, string>();

        public void PollFullTables(ITableauRepoConn connection, ICollection<RepoTable> tables)
        {
            if (connection == null)
            {
                Log.Error("Missing Tableau Repo connection while polling full tables!");
                return;
            }

            Log.Info("Polling Tableau repository tables.");
            tables.Where(t => t.Full)
                .Select(t => t.Name)
                .ToList()
                .ForEach(t =>
                {
                    try
                    {
                        DataTable table = connection.GetTable(t);
                        OutputSerializer.Write(table, true);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        if (ioe.Message.Contains("Connection property has not been initialized"))
                        {
                            // This might also mean that the connection to Tableau is down
                            Log.Warn(ioe, "Temporarily unable to poll full table: '{0}'! Exception: ", t);
                        }
                        else
                        {
                            Log.Error(ioe, "Invalid operation exception while polling full table: '{0}'! Exception: ", t);
                        }
                    }
                });
        }

        private string GetMaxId(string tableName)
        {
            string localMax = this.GetLocalMaxId(tableName);
            try
            {
                // Ask web service what is the max id
                var maxIdPromise = APIClient.GetMaxId(tableName);
                maxIdPromise.Wait();
                string maxIdResult = maxIdPromise.Result;
                if (maxIdResult != null)
                {
                    // TrimEnd removes trailing newline ( + whitespaces )
                    string maxIdFromServer = maxIdResult.TrimEnd();
                    if (RepoPollAgent.CompareMaxIds(localMax, maxIdFromServer) <= 0)
                    {
                        // Max ID coming from the Insight Server is greater or equal than the local one, so use that because it means
                        // that the server has already processed this table up to the max ID coming from the server
                        return maxIdFromServer;
                    }
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is HttpRequestException || x is TaskCanceledException || x is TemporaryException)
                    {
                        // HttpRequestException is expected on network errors. TaskCanceledException is thrown if the async task (HTTP request) timed out.
                        return true;
                    }

                    Log.Warn(x, "Async exception caught while getting max ID for table: {0}! Exception: ", tableName);
                    return false;
                });
            }
            catch (HttpRequestException)
            {
                // Request to the server failed. Just pass back the local max ID
            }

            Log.Warn("Using local max ID: '{0}' for table: '{1}'", localMax, tableName);
            return localMax;
        }

        private string GetLocalMaxId(string tableName)
        {
            string maxId;
            if (RepoPollAgent.localMaxId.TryGetValue(tableName, out maxId))
            {
                return maxId;
            }

            return null;
        }

        internal static int CompareMaxIds(string maxIdA, string maxIdB)
        {
            long numIdA, numIdB;
            if (Int64.TryParse(maxIdA, out numIdA) && Int64.TryParse(maxIdB, out numIdB))
            {
                if (numIdA > numIdB)
                {
                    return 1;
                }
                else if (numIdB > numIdA)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return String.Compare(maxIdA, maxIdB);
            }
        }

        public void PollStreamingTables(ITableauRepoConn connection, ICollection<RepoTable> tables, IOutput output)
        {
            if (connection == null)
            {
                Log.Error("Missing Tableau Repo connection while polling streaming tables!");
                return;
            }

            Log.Info("Polling Tableau Streaming tables.");
            tables.Where(table => !table.Full)
                .ToList()
                .ForEach((table) =>
                {
                    var tableName = table.Name;

                    try
                    {
                        // If we have a pending request for this table, then just skip this iteration
                        if (output.IsInProgress(tableName))
                        {
                            return;
                        }

                        // Get maxid from remote server
                        var maxId = this.GetMaxId(tableName);

                        // Get data from that max id
                        string newMax;
                        DataTable dataTable = connection.GetStreamingTable(tableName, table, maxId, out newMax);
                        Log.Info("Polled records of streaming table {0} from {1} to {2}", tableName, maxId, newMax);
                        if (dataTable != null)
                        {
                            RepoPollAgent.localMaxId[tableName] = newMax;
                            OutputSerializer.Write(dataTable, false, newMax);
                        }
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle((x) =>
                        {
                            if (x is HttpRequestException || x is TaskCanceledException || x is TemporaryException)
                            {
                                // HttpRequestException is expected on network errors. TaskCanceledException is thrown if the async task (HTTP request) timed out.
                                Log.Warn(x, "Polling streaming table: '{0}' timed out! Exception: ", tableName);
                                return true;
                            }

                            Log.Error(x, "Async exception caught while polling streaming table: {0}! Exception: ", tableName);
                            return true;
                        });
                    }
                    catch (TemporaryException tex)
                    {
                        Log.Warn("Temporarily unable to poll streaming table: {0}! Exception: {1}", tableName, tex);
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
