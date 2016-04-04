using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.ComponentModel;
using PaletteInsightAgent.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;

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

            Queue q = new Queue();
            messagesToSplunk = Queue.Synchronized(q);

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


        private Queue           messagesToSplunk;
        private EventWaitHandle stopSign;
        private EventWaitHandle hasMessageToLog;
        private Thread          SplunkerThread;
        private bool            isStopping;

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

                while (messagesToSplunk.Count > MaxPendingQueueSize)
                {
                    try
                    {
                        // Discard the oldest messages, until we get back below the limit.
                        messagesToSplunk.Dequeue();
                    }
                    catch (InvalidOperationException)
                    {
                        // Just go on.
                    }
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
                    messageBatch[i] = (string)messagesToSplunk.Dequeue();
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

                    PostSplunkEvent(byteArray);
                }
                catch (Exception)
                {
                    // TODO: Retry log message on connection error
                    continue;
                }
            }
        }

        private async void PostSplunkEvent(byte[] splunkEvent)
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
                        //if (response.StatusCode != HttpStatusCode.OK)
                        //{
                        //    Console.WriteLine(String.Format("POST unsuccessful. Response: {0}", response));
                        //}
                    }
                }
                catch (Exception)
                {
                    // TODO : Retry Splunk event POST based in case of network outage
                }
            }
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
