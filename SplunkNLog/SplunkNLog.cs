using System;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace SplunkNLog
{
    public class SplunkMessage
    {
        public string Event { get; set; }
    }

    // We need the key names in lowercase while sending events to splunk
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    [Target("SplunkNLog")]
    public sealed class SplunkNLog : TargetWithLayout
    {
        public SplunkNLog()
        {
            //this.Host = "http://52.91.25.176/";
            //this.Port = 4433;
            //this.Token = "376C2EDF-8B36-4F62-9673-658235E25217";
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

            ((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            request.Method = "POST";

            SplunkMessage spm = new SplunkMessage();
            spm.Event = logMessage;
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new LowercaseContractResolver();
            string jsonContent = JsonConvert.SerializeObject(spm, settings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            response.Close();
        }
    }
}
