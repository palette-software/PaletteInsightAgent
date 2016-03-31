using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Net;
using System.IO;
using System.Text;

namespace SplunkNLog
{
    public class SplunkMessage
    {
        public string Event { get; set; }
    }

    [Target("SplunkNLog")]
    public sealed class SplunkNLog : TargetWithLayout
    {
        public SplunkNLog()
        {
        }

        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public int Port { get; set; }

        [RequiredParameter]
        public string Token { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);

            WebRequest request = WebRequest.Create(String.Format("{0}:{1}/services/collector/event", this.Host, this.Port));
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Headers.Add("Authorization", String.Format("Splunk {0}", Token));

            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            request.Method = "POST";

            SplunkMessage spm = new SplunkMessage();
            spm.Event = logMessage;

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

        //protected override void CloseTarget()
        //{
        //    base.CloseTarget();
        //}
    }
}
