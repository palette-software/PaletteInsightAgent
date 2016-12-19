using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Threading;
using System.ComponentModel;
using PaletteInsightAgent.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using fastJSON;

namespace SplunkNLog
{
    public class SplunkMessage
    {
        public string Event { get; set; }
        public string Host { get; set; }
    }

    [Target("SplunkNLog")]
    public sealed class SplunkNLog : TargetWithLayout
    {
        public SplunkNLog()
        {
            isStopping = false;
            try
            {
                machineName = System.Environment.MachineName;
            }
            catch (Exception)
            {
                // Failed to obtain machine name
                machineName = "Unknown machine";
            }

            messagesToSplunk = new ConcurrentQueue<string>();

            stopSign = new EventWaitHandle(false, EventResetMode.ManualReset);
            hasMessageToLog = new EventWaitHandle(false, EventResetMode.AutoReset);

            MaxPendingQueueSize = 65000;
            MaxBatchSize = 100;

            SplunkerThread = new Thread(ProcessLogMessages);
            // Make sure that the worker thread is configured as a background thread,
            // otherwise it will be a frontend thread, and it will make the agent
            // not being able to stop.
            SplunkerThread.IsBackground = true;
            SplunkerThread.Start();
        }

        ~SplunkNLog()
        {
            Dispose();
        }

        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public int Port { get; set; }

        [RequiredParameter]
        public string Token { get; set; }

        [DefaultValue(65000), RequiredParameter]
        public int MaxPendingQueueSize { get; set; }

        [DefaultValue(100), RequiredParameter]
        public int MaxBatchSize { get; set; }


        private string                  machineName;
        private ConcurrentQueue<string> messagesToSplunk;
        private EventWaitHandle         stopSign;
        private EventWaitHandle         hasMessageToLog;
        private Thread                  SplunkerThread;
        private bool                    isStopping;

        private void ProcessLogMessages()
        {
            WaitHandle[] handles = { stopSign, hasMessageToLog };

            while (true)
            {
                int index = WaitHandle.WaitAny(handles);
                if (index == 0)
                {
                    // Stop signal received. Quit.
                    return;
                }

                int messageCount = messagesToSplunk.Count;
                if (messageCount <= 0)
                {
                    continue;
                }

                if (messageCount > MaxPendingQueueSize)
                {
                    // Discard the oldest messages, until we get back below the limit.
                    messagesToSplunk.DiscardItems(messageCount - MaxPendingQueueSize);
                }

                try
                {
                    MemoryStream ms = new MemoryStream();
                    JSONParameters param = new JSONParameters();
                    param.SerializeToLowerCaseNames = true;
                    param.UseExtensions = false;
                    using (StreamWriter writer = new StreamWriter(ms))
                    {
                        var serializedMessagesCount = 0;
                        while (messageCount > 0)
                        {
                            SplunkMessage spm = new SplunkMessage();
                            try
                            {
                                spm.Host = machineName;
                                spm.Event = messagesToSplunk.Dequeue();
                                string jsonContent = JSON.ToJSON(spm, param);
                                writer.WriteLine(jsonContent);
                            }
                            catch (Exception)
                            {
                                // Either dequeue failed (which means that the queue was empty) or the
                                // JSON conversion failed. Either way, skip this event.
                                continue;
                            }
                            finally
                            {
                                serializedMessagesCount++;
                            }

                            // Do not work with too large batches
                            if (serializedMessagesCount >= MaxBatchSize)
                            {
                                // Signal that we still have unprocessed messages.
                                hasMessageToLog.Set();
                                break;
                            }

                            // Try to be greedy to increase batch size.
                            messageCount = messagesToSplunk.Count;
                        }
                    }

                    if (isStopping)
                    {
                        // Shutting down. Abort.
                        return;
                    }

                    // Try to send messages to Splunk, until we have space in the buffer.
                    // This is the way we are handling network issues.
                    while (messagesToSplunk.Count < MaxPendingQueueSize)
                    {
                        var result = PostSplunkEvent(ms.GetBuffer());
                        result.Wait();
                        if (result.Result)
                        {
                            /// Successfully posted message to Splunk. No need to re-try.
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Posts the given byte array to the Splunk server.
        /// </summary>
        /// <param name="splunkEvent"></param>
        /// <returns>This function returns true, if the post operation was successful, or it is
        /// expected to end up with the same result always. If this function returns false, it 
        /// might be wise to re-try the post operation.</returns>
        private async Task<bool> PostSplunkEvent(byte[] splunkEvent)
        {
            using (var handler = APIClient.GetHttpClientHandler())
            using (var httpClient = new HttpClient(handler))
            {
                var authHeader = new AuthenticationHeaderValue("Splunk", Token);
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                try
                {
                    using (var response = await httpClient.PostAsync(String.Format("{0}:{1}/services/collector/event", this.Host, this.Port), new ByteArrayContent(splunkEvent)))
                    {
                        if (response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode >= HttpStatusCode.InternalServerError)
                        {
                            // Retry in case of timed out requests or in case of server errors.
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException || ex is TaskCanceledException)
                    {
                        return false;
                    }
                    // We believe that this exception would be permanent, so there is no point
                    // in re-trying in this case.
                }
            }

            return true;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);
            messagesToSplunk.Enqueue(logMessage);
            hasMessageToLog.Set();
        }

        protected override void CloseTarget()
        {
            isStopping = true;
            stopSign.Set();

            base.CloseTarget();

            SplunkerThread.Join(5000);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            isStopping = true;
            stopSign.Set();
        }
    }

}

public static class ConcurrentQueueExtensions
{
    /// <summary>
    /// Dequeue the next item from the queue. It is a blocking function until
    /// the item is retrieved from the queue. Calling this function on an
    /// empty queue raises an InvalidOperationException.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queue"></param>
    /// <returns>The oldest item of the queue.</returns>
    public static T Dequeue<T>(this ConcurrentQueue<T> queue)
    {
        T item;
        while (!queue.TryDequeue(out item))
        {
            if (queue.Count == 0)
            {
                throw new InvalidOperationException("The queue is already empty. No item can be dequeued.");
            }
            // Try again, we will get it for sure sometime.
        }

        return item;
    }

    /// <summary>
    /// Removes a given number of items from the beginning of the queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queue"></param>
    /// <param name="itemCount"></param>
    public static void DiscardItems<T>(this ConcurrentQueue<T> queue, int itemCount)
    {
        for (int i = 0; i < itemCount; i++)
        {
            try
            {
                queue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // The queue is already empty.
                return;
            }
        }
    }
}
