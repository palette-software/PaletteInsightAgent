using NLog;
using PaletteInsightAgent.Output.OutputDrivers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Helpers
{
    public class APIClient : IDisposable
    {
        public static readonly string API_VERSION = "v1";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static WebserviceConfiguration config = null;
        private static readonly string HostName = Uri.EscapeDataString(Dns.GetHostName());
        private static IWebProxy proxy = null;

        private HttpClient httpClient;
        private HttpClientHandler clientHandler;

        public static void SetTrustSSL()
        {
            ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => {
                    return true;
                    };
        }

        public static void Init(WebserviceConfiguration webConfig)
        {
            config = webConfig;
            if (webConfig.UseProxy)
            {
                proxy = new WebProxy(webConfig.ProxyAddress, false, new string[]{}, new NetworkCredential(webConfig.ProxyUsername, webConfig.ProxyPassword));
            }
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

        public APIClient()
        {
            clientHandler = GetHttpClientHandler();
            httpClient = new HttpClient(clientHandler);

            // Setup the token based authorization automatically
            var authHeader = new AuthenticationHeaderValue("Token", config.AuthToken);
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return httpClient.GetAsync(requestUri);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return httpClient.PostAsync(requestUri, content);
        }

        public void Dispose()
        {
            httpClient.Dispose();
            clientHandler.Dispose();
        }

        private static void VerifyStatusCode(HttpStatusCode statusCode, Action errorDelegate)
        {
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.Unauthorized:
                    throw new InsightUnauthorizedException("Unauthorized access to Insight Server!");
                case HttpStatusCode.BadGateway:
                    throw new TemporaryException("Bad gateway. Insight server is probably getting updated.");
                case HttpStatusCode.Forbidden:
                    throw new TemporaryException("Forbidden. This is probably due to temporary networking issues.");
                default:
                    errorDelegate();
                    return;
            }
        }

        public static async Task<string> GetMaxId(string tableName)
        {
            using (var apiClient = new APIClient())
            {   
                using (var response = await apiClient.GetAsync(GetMaxIdUrl(tableName)))
                {
                    VerifyStatusCode(response.StatusCode, () =>
                    {
                        throw new HttpRequestException(String.Format("Couldn't get max id for table: {0}, Response: {1}", tableName, response.ReasonPhrase));
                    });

                    using (HttpContent content = response.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
        }

        public static async Task<string> CheckLicense(string licenseKey)
        {
            using (var apiClient = new APIClient())
            {
                using (var response = await apiClient.GetAsync(GetLicenseCheckUrl()))
                {
                    VerifyStatusCode(response.StatusCode, () =>
                    {
                        throw new HttpRequestException(String.Format("Couldn't validate license key: {0}, Status code: {1}, Response: {2}",
                                licenseKey, response.StatusCode, response.ReasonPhrase));
                    });

                    using (HttpContent content = response.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
        }

        public static async Task UploadFile(string file, string maxId)
        {
            using (var apiClient = new APIClient())
            {
                var uploadUrl = UploadUrl("public", maxId);
                using (var response = await apiClient.PostAsync(uploadUrl, CreateRequestContents(file)))
                {
                    VerifyStatusCode(response.StatusCode, () =>
                    {
                        throw new ArgumentException(String.Format("-> Unknown status: '{0}' for '{1}' -- moving to error", response.StatusCode, file));
                    });

                    return;
                }
            }
        }

        private static string UploadUrl(string package, string maxId)
        {
            // Get the timezone on each send, so that if the server clock timezone is
            // changed while the agent is running, we are keeping up with the changes
            var timezoneName = DateTimeConverter.WindowsToIana( TimeZoneInfo.Local.Id );
            var url = String.Format("{0}/upload?pkg={1}&host={2}&tz={3}&compression=gzip",
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

        private static string GetLicenseCheckUrl()
        {
            return String.Format("{0}/api/{1}/license", config.Endpoint, API_VERSION);
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
    }

    /// <summary>
    /// This type of exceptions are supposed to be "auto-healing" exceptions. So retry attempts of those operations
    /// which throw exceptions like this, are expected to finish without exception eventually.
    /// </summary>
    class TemporaryException : Exception
    {
        public TemporaryException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// This exception is raised when Insight Server responses "Unauthorized"
    /// </summary>
    class InsightUnauthorizedException : Exception
    {
        public InsightUnauthorizedException(string message) : base(message)
        {
        }
    }
}
