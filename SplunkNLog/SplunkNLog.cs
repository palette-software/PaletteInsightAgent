using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Text;
using System.Threading;
using System.ComponentModel;
using PaletteInsightAgent.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SplunkNLog
{
    public class SplunkMessage
    {
        public string[] Event { get; set; }
    }

    [Target("SplunkNLog")]
    public sealed class SplunkNLog : TargetWithLayout
    {
        public SplunkNLog()
        {
            isStopping = false;

            messagesToSplunk = new ConcurrentQueue<string>();

            stopSign = new EventWaitHandle(false, EventResetMode.ManualReset);
            hasMessageToLog = new EventWaitHandle(false, EventResetMode.AutoReset);

            MaxPendingQueueSize = 65000;
            MaxBatchSize = 100;

            SplunkerThread = new Thread(ProcessLogMessages);
            SplunkerThread.IsBackground = true;
            SplunkerThread.Start();
        }

        ~SplunkNLog()
        {
            Dispose();
        }

        private static readonly int SPLUNK_RETRY_COUNT = 10;

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

                if (messagesToSplunk.Count > MaxPendingQueueSize)
                {
                    // Discard the oldest messages, until we get back below the limit.
                    messagesToSplunk.DiscardItems(messagesToSplunk.Count - MaxPendingQueueSize);
                }

                int messageCount = messagesToSplunk.Count;
                if (messageCount <= 0)
                {
                    continue;
                }

                // Do not work with too large batches
                if (messageCount > MaxBatchSize) messageCount = MaxBatchSize;

                string[] messageBatch = new string[messageCount];
                for (int i = 0; i < messageCount; ++i)
                {
                    try
                    {
                        messageBatch[i] = messagesToSplunk.Dequeue();
                    }
                    catch (InvalidOperationException)
                    {
                        // Failed to dequeue anything. This shouldn't happen, because it means
                        // that the queue got empty somehow. But we are supposed to be the
                        // only thread consuming this queue... Anyway, put an empty string there.
                        messageBatch[i] = "";
                    }
                }

                if (isStopping)
                {
                    // Shutting down. Abort.
                    return;
                }

                try
                {
                    SplunkMessage spm = new SplunkMessage();
                    spm.Event = messageBatch;

                    fastJSON.JSONParameters param = new fastJSON.JSONParameters();
                    param.SerializeToLowerCaseNames = true;
                    param.UseExtensions = false;
                    string jsonContent = fastJSON.JSON.ToJSON(spm, param);
                    byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);

                    for (int i = 0; i < SPLUNK_RETRY_COUNT; i++)
                    {
                        var result = PostSplunkEvent(byteArray);
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
                        if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                        {
                            return false;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    return false;
                }
                catch (Exception)
                {
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
