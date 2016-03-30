// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal.NetworkSenders;
    using NLog.Layouts;
    using Config;
    using System.Net;
    using System.IO;
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json;    /// <summary>
                              /// Sends log messages over the network.
                              /// </summary>
                              /// <seealso href="https://github.com/nlog/nlog/wiki/Network-target">Documentation on NLog Wiki</seealso>
                              /// <example>
                              /// <p>
                              /// To set up the target in the <a href="config.html">configuration file</a>, 
                              /// use the following syntax:
                              /// </p>
                              /// <code lang="XML" source="examples/targets/Configuration File/Network/NLog.config" />
                              /// <p>
                              /// This assumes just one target and a single rule. More configuration
                              /// options are described <a href="config.html">here</a>.
                              /// </p>
                              /// <p>
                              /// To set up the log target programmatically use code like this:
                              /// </p>
                              /// <code lang="C#" source="examples/targets/Configuration API/Network/Simple/Example.cs" />
                              /// <p>
                              /// To print the results, use any application that's able to receive messages over
                              /// TCP or UDP. <a href="http://m.nu/program/util/netcat/netcat.html">NetCat</a> is
                              /// a simple but very powerful command-line tool that can be used for that. This image
                              /// demonstrates the NetCat tool receiving log messages from Network target.
                              /// </p>
                              /// <img src="examples/targets/Screenshots/Network/Output.gif" />
                              /// <p>
                              /// NOTE: If your receiver application is ever likely to be off-line, don't use TCP protocol
                              /// or you'll get TCP timeouts and your application will be very slow. 
                              /// Either switch to UDP transport or use <a href="target.AsyncWrapper.html">AsyncWrapper</a> target
                              /// so that your application threads will not be blocked by the timing-out connection attempts.
                              /// </p>
                              /// <p>
                              /// There are two specialized versions of the Network target: <a href="target.Chainsaw.html">Chainsaw</a>
                              /// and <a href="target.NLogViewer.html">NLogViewer</a> which write to instances of Chainsaw log4j viewer
                              /// or NLogViewer application respectively.
                              /// </p>
                              /// </example>
    [Target("SplunkNLog")]
    public class SplunkNLogTarget : TargetWithLayout
    {
        private Dictionary<string, LinkedListNode<NetworkSender>> currentSenderCache = new Dictionary<string, LinkedListNode<NetworkSender>>();
        private LinkedList<NetworkSender> openNetworkSenders = new LinkedList<NetworkSender>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkNLogTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public SplunkNLogTarget()
        {
            //this.SenderFactory = NetworkSenderFactory.Default;
            this.Encoding = Encoding.UTF8;
            this.OnOverflow = NetworkTargetOverflowAction.Split;
            this.KeepConnection = true;
            this.MaxMessageSize = 65000;
            this.ConnectionCacheSize = 5;
        }

        ///// <summary>
        ///// Gets or sets the network address.
        ///// </summary>
        ///// <remarks>
        ///// The network address can be:
        ///// <ul>
        ///// <li>http://host:port/pageName - HTTP using POST verb</li>
        ///// <li>https://host:port/pageName - HTTPS using POST verb</li>
        ///// </ul>
        ///// </remarks>
        ///// <docgen category='Connection Options' order='10' />
        //public Layout Address { get; set; }

        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public int Port { get; set; }

        [RequiredParameter]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep connection open whenever possible.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(true)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append newline at the end of log message.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(false)]
        public bool NewLine { get; set; }

        /// <summary>
        /// Gets or sets the maximum message size in bytes.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(65000)]
        public int MaxMessageSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the connection cache (number of connections which are kept alive).
        /// </summary>
        /// <docgen category="Connection Options" order="10"/>
        [DefaultValue(1)]
        public int ConnectionCacheSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum current connections. 0 = no maximum.
        /// </summary>
        /// <docgen category="Connection Options" order="10"/>
        [DefaultValue(1)]
        public int MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the will be more connections than <see cref="MaxConnections"/>.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("Block")]
        public NetworkTargetConnectionsOverflowAction OnConnectionOverflow { get; set; }

        ///// <summary>
        ///// Gets or sets the maximum queue size.
        ///// </summary>
        //[DefaultValue(0)]
        //public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the message is larger than
        /// maxMessageSize.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("Split")]
        public NetworkTargetOverflowAction OnOverflow { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("utf-8")]
        public Encoding Encoding { get; set; }

        //internal INetworkSenderFactory SenderFactory { get; set; }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            int remainingCount = 0;

            AsyncContinuation continuation =
                ex =>
                {
                    // ignore exception
                    if (Interlocked.Decrement(ref remainingCount) == 0)
                    {
                        asyncContinuation(null);
                    }
                };

            lock (this.openNetworkSenders)
            {
                remainingCount = this.openNetworkSenders.Count;
                if (remainingCount == 0)
                {
                    // nothing to flush
                    asyncContinuation(null);
                }
                else
                {
                    // otherwise call FlushAsync() on all senders
                    // and invoke continuation at the very end
                    foreach (var openSender in this.openNetworkSenders)
                    {
                        openSender.FlushAsync(continuation);
                    }
                }
            }
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            lock (this.openNetworkSenders)
            {
                foreach (var openSender in this.openNetworkSenders)
                {
                    openSender.Close(ex => { });
                }

                this.openNetworkSenders.Clear();
            }
        }

        /// <summary>
        /// Sends the 
        /// rendered logging event over the network optionally concatenating it with a newline character.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            //string address = this.Address.Render(logEvent.LogEvent);
            string address = String.Format("{0}:{1}/services/collector/event", this.Host, this.Port);
            string logMessage = this.Layout.Render(logEvent.LogEvent);

            //WebRequest request = WebRequest.Create(String.Format("{0}:{1}/services/collector/event", this.Host, this.Port));
            //request.Credentials = CredentialCache.DefaultCredentials;
            //request.Headers.Add("Authorization", String.Format("Splunk {0}", Token));

            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            //request.Method = "POST";

            //SplunkMessage spm = new SplunkMessage();
            //spm.Event = logMessage;
            ////var settings = new JsonSerializerSettings();
            ////settings.ContractResolver = new LowercaseContractResolver();
            ////string jsonContent = JsonConvert.SerializeObject(spm, settings);
            //string jsonContentRaw = JsonConvert.SerializeObject(spm);
            //string jsonContent = jsonContentRaw.Replace("\"Event\"", "\"event\"");

            ////string jsonContent = String.Format("{\"event\":\"{0}\"}", logMessage);
            ////string jsonContent = "{\"event\":\"Hello, World!\"}";
            //byte[] bytes = Encoding.UTF8.GetBytes(jsonContent);

            //request.ContentLength = byteArray.Length;
            //request.ContentType = "application/x-www-form-urlencoded";


            //Stream dataStream = request.GetRequestStream();
            //dataStream.Write(byteArray, 0, byteArray.Length);
            //dataStream.Close();

            //Stream byteStream = new MemoryStream();
            //byteStream.Write()


            //byte[] bytes = this.GetBytesToWrite(logEvent.LogEvent);
            byte[] bytes = Encoding.UTF8.GetBytes(logMessage);

            if (this.KeepConnection)
            {
                var senderNode = this.GetCachedNetworkSender(address);

                this.ChunkedSend(
                    senderNode.Value,
                    logMessage,
                    bytes,
                    ex =>
                    {
                        if (ex != null)
                        {
                            InternalLogger.Error(ex.Message, "Error when sending.");
                            this.ReleaseCachedConnection(senderNode);
                        }

                        logEvent.Continuation(ex);
                    });
            }
            else
            {

                NetworkSender sender;
                LinkedListNode<NetworkSender> linkedListNode;

                lock (this.openNetworkSenders)
                {
                    //handle too many connections
                    var tooManyConnections = this.openNetworkSenders.Count >= MaxConnections;

                    if (tooManyConnections && MaxConnections > 0)
                    {
                        switch (this.OnConnectionOverflow)
                        {
                            case NetworkTargetConnectionsOverflowAction.DiscardMessage:
                                InternalLogger.Warn("Discarding message otherwise to many connections.");
                                logEvent.Continuation(null);
                                return;

                            case NetworkTargetConnectionsOverflowAction.AllowNewConnnection:
                                InternalLogger.Debug("Too may connections, but this is allowed");
                                break;

                            case NetworkTargetConnectionsOverflowAction.Block:
                                while (this.openNetworkSenders.Count >= this.MaxConnections)
                                {
                                    InternalLogger.Debug("Blocking networktarget otherwhise too many connections.");
                                    System.Threading.Monitor.Wait(this.openNetworkSenders);
                                    InternalLogger.Trace("Entered critical section.");
                                }

                                InternalLogger.Trace("Limit ok.");
                                break;
                        }
                    }

                    //sender = this.SenderFactory.Create(address, MaxQueueSize);
                    sender = new HttpNetworkSender(address, Token);
                    sender.Initialize();

                    linkedListNode = this.openNetworkSenders.AddLast(sender);
                }
                this.ChunkedSend(
                    sender,
                    logMessage,
                    bytes,
                    ex =>
                    {
                        lock (this.openNetworkSenders)
                        {
                            TryRemove(this.openNetworkSenders, linkedListNode);
                            if (this.OnConnectionOverflow == NetworkTargetConnectionsOverflowAction.Block)
                            {
                                System.Threading.Monitor.PulseAll(this.openNetworkSenders);
                            }
                        }

                        if (ex != null)
                        {
                            InternalLogger.Error(ex.Message, "Error when sending.");
                        }

                        sender.Close(ex2 => { });
                        logEvent.Continuation(ex);
                    });

            }
        }

        /// <summary>
        /// Try to remove. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <returns>removed something?</returns>
        private static bool TryRemove<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            if (node == null || list != node.List)
            {
                return false;
            }
            list.Remove(node);
            return true;
        }

        /// <summary>
        /// Gets the bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text;

            if (this.NewLine)
            {
                text = this.Layout.Render(logEvent) + "\r\n";
            }
            else
            {
                text = this.Layout.Render(logEvent);
            }

            return this.Encoding.GetBytes(text);
        }

        private LinkedListNode<NetworkSender> GetCachedNetworkSender(string address)
        {
            lock (this.currentSenderCache)
            {
                LinkedListNode<NetworkSender> senderNode;

                // already have address
                if (this.currentSenderCache.TryGetValue(address, out senderNode))
                {
                    senderNode.Value.CheckSocket();
                    return senderNode;
                }

                if (this.currentSenderCache.Count >= this.ConnectionCacheSize)
                {
                    // make room in the cache by closing the least recently used connection
                    int minAccessTime = int.MaxValue;
                    LinkedListNode<NetworkSender> leastRecentlyUsed = null;

                    foreach (var pair in this.currentSenderCache)
                    {
                        var networkSender = pair.Value.Value;
                        if (networkSender.LastSendTime < minAccessTime)
                        {
                            minAccessTime = networkSender.LastSendTime;
                            leastRecentlyUsed = pair.Value;
                        }
                    }

                    if (leastRecentlyUsed != null)
                    {
                        this.ReleaseCachedConnection(leastRecentlyUsed);
                    }
                }

                //var sender = this.SenderFactory.Create(address, MaxQueueSize);
                var sender = new HttpNetworkSender(address, Token);
                sender.Initialize();
                lock (this.openNetworkSenders)
                {
                    senderNode = this.openNetworkSenders.AddLast(sender);
                }

                this.currentSenderCache.Add(address, senderNode);
                return senderNode;
            }
        }

        private void ReleaseCachedConnection(LinkedListNode<NetworkSender> senderNode)
        {
            lock (this.currentSenderCache)
            {
                var networkSender = senderNode.Value;
                lock (this.openNetworkSenders)
                {

                    if (TryRemove(this.openNetworkSenders, senderNode))
                    {
                        // only remove it once
                        networkSender.Close(ex => { });
                    }
                }

                LinkedListNode<NetworkSender> sender2;

                // make sure the current sender for this address is the one we want to remove
                if (this.currentSenderCache.TryGetValue(networkSender.Address, out sender2))
                {
                    if (ReferenceEquals(senderNode, sender2))
                    {
                        this.currentSenderCache.Remove(networkSender.Address);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using property names in message.")]
        private void ChunkedSend(NetworkSender sender, string message, byte[] buffer, AsyncContinuation continuation)
        {
            int tosend = buffer.Length;
            int pos = 0;

            AsyncContinuation sendNextChunk = null;

            sendNextChunk = ex =>
            {
                if (ex != null)
                {
                    continuation(ex);
                    return;
                }

                if (tosend <= 0)
                {
                    continuation(null);
                    return;
                }

                int chunksize = tosend;
                if (chunksize > this.MaxMessageSize)
                {
                    if (this.OnOverflow == NetworkTargetOverflowAction.Discard)
                    {
                        continuation(null);
                        return;
                    }

                    if (this.OnOverflow == NetworkTargetOverflowAction.Error)
                    {
                        continuation(new OverflowException("Attempted to send a message larger than MaxMessageSize (" + this.MaxMessageSize + "). Actual size was: " + buffer.Length + ". Adjust OnOverflow and MaxMessageSize parameters accordingly."));
                        return;
                    }

                    chunksize = this.MaxMessageSize;
                }

                int pos0 = pos;
                tosend -= chunksize;
                pos += chunksize;

                sender.Send(message, buffer, pos0, chunksize, sendNextChunk);
            };

            sendNextChunk(null);
        }
    }


    //public class SplunkMessage
    //{
    //    public string Event { get; set; }
    //}

    //// We need the key names in lowercase while sending events to splunk
    //public class LowercaseContractResolver : DefaultContractResolver
    //{
    //    protected override string ResolvePropertyName(string propertyName)
    //    {
    //        return propertyName.ToLower();
    //    }
    //}
}



//////////////// SplunkNLog
//using System;
//using System.Collections;
//using NLog;
//using NLog.Targets;
//using NLog.Config;
//using System.Net;
//using System.IO;
//using Newtonsoft.Json;
//using System.Text;
//using Newtonsoft.Json.Serialization;
//using System.Threading;

//namespace SplunkNLog
//{
//    public class SplunkMessage
//    {
//        public string Event { get; set; }
//    }

//    We need the key names in lowercase while sending events to splunk
//    public class LowercaseContractResolver : DefaultContractResolver
//    {
//        protected override string ResolvePropertyName(string propertyName)
//        {
//            return propertyName.ToLower();
//        }
//    }

//    [Target("SplunkNLog")]
//    public sealed class SplunkNLog : TargetWithLayout
//    {
//        public SplunkNLog()
//        {
//            this.Host = "http://52.91.25.176/";
//            this.Port = 4433;
//            this.Token = "376C2EDF-8B36-4F62-9673-658235E25217";

//            Queue q = new Queue();
//            Queue messagesToSplunk = Queue.Synchronized(q);

//            this.SplunkerThread = new Thread(SplunkMessages);
//            this.SplunkerThread.Start();
//        }




//        [RequiredParameter]
//        public string Host { get; set; }

//        [RequiredParameter]
//        public int Port { get; set; }

//        [RequiredParameter]
//        public string Token { get; set; }

//        private bool stopSign;
//        private Queue messagesToSplunk;
//        private Thread SplunkerThread;

//        private void SplunkMessages()
//        {
//            while (!stopSign)
//            {

//            }
//        }

//        protected override void Write(LogEventInfo logEvent)
//        {
//            bool trigger = true;
//            while (trigger)
//            {
//                int hooked = 0;
//            }
//            try
//            {
//                string logMessage = this.Layout.Render(logEvent);

//                WebRequest request = WebRequest.Create(String.Format("{0}:{1}/services/collector/event", this.Host, this.Port));
//                request.Credentials = CredentialCache.DefaultCredentials;
//                request.Headers.Add("Authorization", String.Format("Splunk {0}", Token));

//                ((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
//                request.Method = "POST";

//                SplunkMessage spm = new SplunkMessage();
//                spm.Event = logMessage;
//                var settings = new JsonSerializerSettings();
//                settings.ContractResolver = new LowercaseContractResolver();
//                string jsonContent = JsonConvert.SerializeObject(spm, settings);

//                string jsonContent = String.Format("{\"event\":\"{0}\"}", logMessage);
//                string jsonContent = "{\"event\":\"Hello, World!\"}";
//                byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);

//                request.ContentLength = byteArray.Length;
//                request.ContentType = "application/x-www-form-urlencoded";

//                Stream dataStream = request.GetRequestStream();
//                dataStream.Write(byteArray, 0, byteArray.Length);
//                dataStream.Close();

//                using (var streamWriter = File.AppendText("splunked.log"))
//                {
//                    streamWriter.WriteLine(logMessage);
//                    streamWriter.WriteLine("Response: {0}", response.ToString());
//                    streamWriter.Flush();
//                }
//            }
//            catch (Exception ex)
//            {
//                using (var streamWriter = File.AppendText("splunkex.log"))
//                {
//                    streamWriter.WriteLine(ex.Message);
//                    streamWriter.WriteLine("Response: {0}", response.ToString());
//                    streamWriter.Flush();
//                }
//                return;
//            }

//            WebResponse response = request.GetResponse();
//            response.Close();
//        }
//    }
//}


////////////// WebService target
//// 
//// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
//// 
//// All rights reserved.
//// 
//// Redistribution and use in source and binary forms, with or without 
//// modification, are permitted provided that the following conditions 
//// are met:
//// 
//// * Redistributions of source code must retain the above copyright notice, 
////   this list of conditions and the following disclaimer. 
//// 
//// * Redistributions in binary form must reproduce the above copyright notice,
////   this list of conditions and the following disclaimer in the documentation
////   and/or other materials provided with the distribution. 
//// 
//// * Neither the name of Jaroslaw Kowalski nor the names of its 
////   contributors may be used to endorse or promote products derived from this
////   software without specific prior written permission. 
//// 
//// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
//// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
//// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
//// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
//// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
//// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
//// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
//// THE POSSIBILITY OF SUCH DAMAGE.
//// 

//using System.Linq;

//namespace NLog.Targets
//{
//    using System;
//    using System.ComponentModel;
//    using System.Globalization;
//    using System.IO;
//    using System.Net;
//    using System.Text;
//    using System.Xml;
//    using NLog.Common;
//    using NLog.Internal;
//    using NLog.Layouts;

//    /// <summary>
//    /// Calls the specified web service on each log message.
//    /// </summary>
//    /// <seealso href="https://github.com/nlog/nlog/wiki/WebService-target">Documentation on NLog Wiki</seealso>
//    /// <remarks>
//    /// The web service must implement a method that accepts a number of string parameters.
//    /// </remarks>
//    /// <example>
//    /// <p>
//    /// To set up the target in the <a href="config.html">configuration file</a>, 
//    /// use the following syntax:
//    /// </p>
//    /// <code lang="XML" source="examples/targets/Configuration File/WebService/NLog.config" />
//    /// <p>
//    /// This assumes just one target and a single rule. More configuration
//    /// options are described <a href="config.html">here</a>.
//    /// </p>
//    /// <p>
//    /// To set up the log target programmatically use code like this:
//    /// </p>
//    /// <code lang="C#" source="examples/targets/Configuration API/WebService/Simple/Example.cs" />
//    /// <p>The example web service that works with this example is shown below</p>
//    /// <code lang="C#" source="examples/targets/Configuration API/WebService/Simple/WebService1/Service1.asmx.cs" />
//    /// </example>
//    [Target("WebService")]
//    public sealed class WebServiceTarget : MethodCallTargetBase
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="WebServiceTarget" /> class.
//        /// </summary>
//        public WebServiceTarget()
//        {
//            this.Protocol = WebServiceProtocol.HttpPost;

//            //default NO utf-8 bom 
//            const bool writeBOM = false;
//            this.Encoding = new UTF8Encoding(writeBOM);
//            this.IncludeBOM = writeBOM;


//        }

//        /// <summary>
//        /// Gets or sets the web service URL.
//        /// </summary>
//        /// <docgen category='Web Service Options' order='10' />
//        public Uri Url { get; set; }

//        /// <summary>
//        /// Gets or sets the Web service method name. Only used with Soap.
//        /// </summary>
//        /// <docgen category='Web Service Options' order='10' />
//        public string MethodName { get; set; }

//        /// <summary>
//        /// Gets or sets the Web service namespace. Only used with Soap.
//        /// </summary>
//        /// <docgen category='Web Service Options' order='10' />
//        public string Namespace { get; set; }

//        /// <summary>
//        /// Gets or sets the protocol to be used when calling web service.
//        /// </summary>
//        /// <docgen category='Web Service Options' order='10' />
//        [DefaultValue("HttpPost")]
//        public WebServiceProtocol Protocol { get; set; }

//        /// <summary>
//        /// Should we include the BOM (Byte-order-mark) for UTF? Influences the <see cref="Encoding"/> property.
//        /// 
//        /// This will only work for UTF-8.
//        /// </summary>
//        public bool? IncludeBOM { get; set; }

//        /// <summary>
//        /// Gets or sets the encoding.
//        /// </summary>
//        /// <docgen category='Web Service Options' order='10' />
//        public Encoding Encoding { get; set; }

//        /// <summary>
//        /// Calls the target method. Must be implemented in concrete classes.
//        /// </summary>
//        /// <param name="parameters">Method call parameters.</param>
//        protected override void DoInvoke(object[] parameters)
//        {
//            // method is not used, instead asynchronous overload will be used
//            throw new NotImplementedException();
//        }


//        /// <summary>
//        /// Invokes the web service method.
//        /// </summary>
//        /// <param name="parameters">Parameters to be passed.</param>
//        /// <param name="continuation">The continuation.</param>
//        protected override void DoInvoke(object[] parameters, AsyncContinuation continuation)
//        {
//            var request = (HttpWebRequest)WebRequest.Create(BuildWebServiceUrl(parameters));
//            Func<AsyncCallback, IAsyncResult> begin = (r) => request.BeginGetRequestStream(r, null);
//            Func<IAsyncResult, Stream> getStream = request.EndGetRequestStream;

//            DoInvoke(parameters, continuation, request, begin, getStream);
//        }

//        internal void DoInvoke(object[] parameters, AsyncContinuation continuation, HttpWebRequest request, Func<AsyncCallback, IAsyncResult> beginFunc,
//            Func<IAsyncResult, Stream> getStreamFunc)
//        {
//            Stream postPayload = null;

//            switch (this.Protocol)
//            {
//                case WebServiceProtocol.HttpGet:
//                    this.PrepareGetRequest(request);
//                    break;

//                case WebServiceProtocol.HttpPost:
//                    postPayload = this.PreparePostRequest(request, parameters);
//                    break;
//            }

//            AsyncContinuation sendContinuation =
//                ex =>
//                {
//                    if (ex != null)
//                    {
//                        continuation(ex);
//                        return;
//                    }

//                    request.BeginGetResponse(
//                        r =>
//                        {
//                            try
//                            {
//                                using (var response = request.EndGetResponse(r))
//                                {
//                                }

//                                continuation(null);
//                            }
//                            catch (Exception ex2)
//                            {
//                                InternalLogger.Error(ex2, "Error when sending to Webservice.");

//                                if (ex2.MustBeRethrown())
//                                {
//                                    throw;
//                                }

//                                continuation(ex2);
//                            }
//                        },
//                        null);
//                };

//            if (postPayload != null && postPayload.Length > 0)
//            {
//                postPayload.Position = 0;
//                beginFunc(
//                    result =>
//                    {
//                        try
//                        {
//                            using (Stream stream = getStreamFunc(result))
//                            {
//                                WriteStreamAndFixPreamble(postPayload, stream, this.IncludeBOM, this.Encoding);

//                                postPayload.Dispose();
//                            }

//                            sendContinuation(null);
//                        }
//                        catch (Exception ex)
//                        {
//                            postPayload.Dispose();
//                            InternalLogger.Error(ex, "Error when sending to Webservice.");

//                            if (ex.MustBeRethrown())
//                            {
//                                throw;
//                            }

//                            continuation(ex);
//                        }
//                    });
//            }
//            else
//            {
//                sendContinuation(null);
//            }
//        }

//        /// <summary>
//        /// Builds the URL to use when calling the web service for a message, depending on the WebServiceProtocol.
//        /// </summary>
//        /// <param name="parameterValues"></param>
//        /// <returns></returns>
//        private Uri BuildWebServiceUrl(object[] parameterValues)
//        {
//            if (this.Protocol != WebServiceProtocol.HttpGet)
//            {
//                return this.Url;
//            }

//            //if the protocol is HttpGet, we need to add the parameters to the query string of the url
//            var queryParameters = new StringBuilder();
//            string separator = string.Empty;
//            for (int i = 0; i < this.Parameters.Count; i++)
//            {
//                queryParameters.Append(separator);
//                queryParameters.Append(this.Parameters[i].Name);
//                queryParameters.Append("=");
//                queryParameters.Append(UrlHelper.UrlEncode(Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture), false));
//                separator = "&";
//            }

//            var builder = new UriBuilder(this.Url);
//            //append our query string to the URL following 
//            //the recommendations at https://msdn.microsoft.com/en-us/library/system.uribuilder.query.aspx
//            if (builder.Query != null && builder.Query.Length > 1)
//            {
//                builder.Query = builder.Query.Substring(1) + "&" + queryParameters.ToString();
//            }
//            else
//            {
//                builder.Query = queryParameters.ToString();
//            }

//            return builder.Uri;
//        }

//        private MemoryStream PreparePostRequest(HttpWebRequest request, object[] parameterValues)
//        {
//            request.Method = "POST";
//            return PrepareHttpRequest(request, parameterValues);
//        }

//        private void PrepareGetRequest(HttpWebRequest request)
//        {
//            request.Method = "GET";
//        }

//        private MemoryStream PrepareHttpRequest(HttpWebRequest request, object[] parameterValues)
//        {
//            request.ContentType = "application/x-www-form-urlencoded; charset=" + this.Encoding.WebName;

//            var ms = new MemoryStream();
//            string separator = string.Empty;
//            var sw = new StreamWriter(ms, this.Encoding);
//            sw.Write(string.Empty);
//            int i = 0;
//            foreach (MethodCallParameter parameter in this.Parameters)
//            {
//                sw.Write(separator);
//                sw.Write(parameter.Name);
//                sw.Write("=");
//                sw.Write(UrlHelper.UrlEncode(Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture), true));
//                separator = "&";
//                i++;
//            }
//            sw.Flush();
//            return ms;
//        }


//        /// <summary>
//        /// Write from input to output. Fix the UTF-8 bom
//        /// </summary>
//        /// <param name="input"></param>
//        /// <param name="output"></param>
//        /// <param name="writeUtf8BOM"></param>
//        /// <param name="encoding"></param>
//        private static void WriteStreamAndFixPreamble(Stream input, Stream output, bool? writeUtf8BOM, Encoding encoding)
//        {
//            //only when utf-8 encoding is used, the Encoding preamble is optional
//            var nothingToDo = writeUtf8BOM == null || !(encoding is UTF8Encoding);

//            const int preambleSize = 3;
//            if (!nothingToDo)
//            {
//                //it's UTF-8
//                var hasBomInEncoding = encoding.GetPreamble().Length == preambleSize;

//                //BOM already in Encoding.
//                nothingToDo = writeUtf8BOM.Value && hasBomInEncoding;

//                //Bom already not in Encoding
//                nothingToDo = nothingToDo || !writeUtf8BOM.Value && !hasBomInEncoding;
//            }
//            var offset = nothingToDo ? 0 : preambleSize;
//            input.CopyWithOffset(output, offset);

//        }


//    }
//}
