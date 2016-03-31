using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.ComponentModel;

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

            SplunkerThread = new Thread(ProcessLogMessages);
            SplunkerThread.Start();
        }

        ~SplunkNLog()
        {
            isStopping = true;
            stopSign.Set();
        }

        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public int Port { get; set; }

        [RequiredParameter]
        public string Token { get; set; }

        [DefaultValue(65000), DefaultParameter]
        public int MaxPendingQueueSize { get; set; }


        private Queue           messagesToSplunk;
        private EventWaitHandle stopSign;
        private EventWaitHandle hasMessageToLog;
        private Thread          SplunkerThread;
        private bool            isStopping;

        private void ProcessLogMessages()
        {
            WaitHandle[] handles = { hasMessageToLog, stopSign };

            while (true)
            {
                int index = WaitHandle.WaitAny(handles);
                if (index == 1)
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

                int maxBatchSize = 100;
                // Do not work with too large batches
                if (messageCount > maxBatchSize) messageCount = maxBatchSize;

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
                    WebRequest request = WebRequest.Create(String.Format("{0}:{1}/services/collector/event", this.Host, this.Port));
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Headers.Add("Authorization", String.Format("Splunk {0}", Token));

                    //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
                    request.Method = "POST";

                    SplunkMessage spm = new SplunkMessage();
                    spm.Event = messageBatch;

                    fastJSON.JSONParameters param = new fastJSON.JSONParameters();
                    param.SerializeToLowerCaseNames = true;
                    param.UseExtensions = false;
                    string jsonContent = fastJSON.JSON.ToJSON(spm, param);

                    byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);

                    request.ContentLength = byteArray.Length;
                    request.ContentType = "application/x-www-form-urlencoded";

                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    // It's important to get the response, otherwise it won't send new requests.
                    WebResponse response = request.GetResponse();
                    response.Close();
                }
                catch (Exception)
                {
                    // TODO: Retry log message on connection error
                    continue;
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
    }
}
