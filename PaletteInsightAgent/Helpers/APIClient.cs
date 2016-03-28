using NLog;
using PaletteInsightAgent.Output.OutputDrivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Helpers
{
    class APIClient
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static WebserviceConfiguration config = null;
        private static readonly string HostName = Uri.EscapeDataString(Dns.GetHostName());
        private static IWebProxy proxy = null;

        public static void Init(WebserviceConfiguration webConfig)
        {
            config = webConfig;
            if (webConfig.UseProxy)
            {
                proxy = new WebProxy(webConfig.ProxyAddress, false, new string[]{}, new NetworkCredential(webConfig.ProxyUsername, webConfig.ProxyPassword));
            }
            ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => {
                    return true;
                    };
        }

        public static HttpClientHandler GetHttpClientHandler()
        {
            var handler = new HttpClientHandler();
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }
            return handler;
        }

        public static async Task<string> GetMaxId(string tableName)
        {
            using (var handler = GetHttpClientHandler())
            using (var httpClient = new HttpClient(handler))
            {
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(CreateAuthHeader()));
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                using (var response = await httpClient.GetAsync(GetMaxIdUrl(tableName)))
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return null;
                    }
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpRequestException(String.Format("Couldn't get max id for table: {0}, Response: {1}", tableName, response.ReasonPhrase));
                    }
                    using (HttpContent content = response.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
        }

        public static async Task<HttpResponseMessage> UploadFile(string file, string maxId)
        {
            using (var handler = GetHttpClientHandler())
            using (var httpClient = new HttpClient(handler))
            {
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(CreateAuthHeader()));
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                var package = "public";
                var uploadUrl = UploadUrl(package, maxId);
                using (var response = await httpClient.PostAsync(uploadUrl, CreateRequestContents(file)))
                {
                    return response;
                }
            }
        }

        private static string UploadUrl(string package, string maxId)
        {
            // Get the timezone on each send, so that if the server clock timezone is
            // changed while the agent is running, we are keeping up with the changes
            var timezoneName = TimezoneHelpers.WindowsToIana( TimeZoneInfo.Local.Id );
            var url = String.Format("{0}/upload?pkg={1}&host={2}&tz={3}",
                config.Endpoint, 
                Uri.EscapeUriString(package), 
                Uri.EscapeUriString(HostName),
                Uri.EscapeUriString(timezoneName));

            if (maxId != null)
            {
                url = String.Format("{0}&maxid={1}", url, Uri.EscapeDataString(maxId));
            }
            return url;
        }

        private static string GetMaxIdUrl(string tableName)
        {
            return String.Format("{0}/maxid?table={1}", config.Endpoint, tableName);
        }

        private static MultipartFormDataContent CreateRequestContents(string file)
        {
            // create the form data for upload
            MultipartFormDataContent form = new MultipartFormDataContent();
            // try to pack the list of files into the request

            var fileContent = File.ReadAllBytes(file);
            var fileBaseName = Path.GetFileName(file);

            byte[] fileHash;
            using (var md5Hasher = MD5.Create())
            {
                fileHash = md5Hasher.ComputeHash(fileContent);
            }

            Log.Debug("+ REQUEST: Adding file: '{0}' as '{1}' - {2} bytes", file, fileBaseName, fileContent.Length);

            // add to the fields
            form.Add(new ByteArrayContent(fileContent), "_file", fileBaseName);
            // we encode the md5 as a base64 value, so we dont have to do tricks on
            // go's side to get this value
            form.Add(new StringContent(Convert.ToBase64String(fileHash)), "_md5");
            return form;
        }

        private static byte[] CreateAuthHeader()
        {
            // add the basic authentication data
            var encoding = new ASCIIEncoding();

            var usernameBytes = encoding.GetBytes(String.Format("{0}:", config.Username));

            var authLen = config.AuthToken.Length;
            var usernameLen = usernameBytes.Length;

            var authBytes = new byte[usernameLen + authLen];

            System.Buffer.BlockCopy(usernameBytes, 0, authBytes, 0, usernameLen);
            System.Buffer.BlockCopy(config.AuthToken, 0, authBytes, usernameLen, authLen);
            return authBytes;

        }
    }
}
