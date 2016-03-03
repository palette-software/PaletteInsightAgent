using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaletteInsightAgent.Output.OutputDrivers
{
    /// <summary>
    /// Shared functionality between Single and Multi file backends
    /// </summary>
    public class WebserviceBackendBase
    {
        public WebserviceConfiguration config;

        /// <summary>
        /// The default encoding for the log files (we use this to read the bytes)
        /// </summary>
        protected static Encoding defaultStringEncoding = new UTF8Encoding();

        /// <summary>
        /// The hash algorith to use for the signature
        /// </summary>
        protected HashAlgorithm fileHasher = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5"));

        /// <summary>
        /// Helper that creates an authenticated httpClient and executes the delegate
        /// and returns its return value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sendDelegate"></param>
        /// <returns></returns>
        protected T WithConnection<T>(Func<HttpClient, T> sendDelegate)
        {
            // create the httpclient
            using (var httpClient = new HttpClient())
            {
                // pack it into a base64 encoded header and use it as the basic http auth
                var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(CreateAuthHeader()));
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                return sendDelegate(httpClient);
            }

        }

        private byte[] CreateAuthHeader()
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

        /// <summary>
        /// Gets the Md5 of a file using a FileStream instead of reading it into memory
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected static string GetFileMd5(string file)
        {
            // get the MD5
            using (var fs = File.OpenRead(file))
            {
                using (var md5 = MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", string.Empty);
                }
            }
        }


        #region IDisposable
        /// <summary>
        /// Since this output driver holds no resources that need to be released,
        /// we dont do anything is Dispose()
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }

}
