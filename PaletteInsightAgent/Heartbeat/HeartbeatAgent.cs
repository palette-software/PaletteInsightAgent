using NLog;
using PaletteInsightAgent.Helpers;

namespace PaletteInsightAgent.Heartbeat
{
    class HeartbeatAgent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // do the heartbeats in 15 second intervals
        public const int HeartbeatInterval = 15 * 1000;
        public static readonly object Lock = new object();

        /// <summary>
        /// Sends a heartbeat to the server
        /// </summary>
        public static void Send()
        {
            Log.Info("[Heartbeat] Sending heartbeat.");
            APIClient.SendHeartbeat();
        }

    }
}
