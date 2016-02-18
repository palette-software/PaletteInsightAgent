using System;
using Topshelf;
using PaletteInsightAgent;
using NLog;

namespace PaletteInsightAgentService
{
    /// <summary>
    /// Serves as a thin bootstrapper for the PaletteInsightAgentAgent class and adapts underlying Stop/Start methods to the service context.
    /// </summary>
    public class PaletteInsightAgentServiceBootstrapper : ServiceControl, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private PaletteInsightAgentAgent agent;
        private bool disposed;

        ~PaletteInsightAgentServiceBootstrapper()
        {
            Dispose(false);
        }

        #region Public Methods

        /// <summary>
        /// Creates an instance of the PaletteInsightAgentAgent and starts it.
        /// </summary>
        /// <param name="hostControl">Service HostControl object</param>
        /// <returns>Indicator that service succesfully started</returns>
        public bool Start(HostControl hostControl)
        {
            // Request additional time from the service host due to how much initialization has to take place.
            hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

            // Initialize and start service.
            try
            {
                agent = new PaletteInsightAgentAgent();
                // move starting the agent here, so exceptions get properly logged not only on construction,
                // but on start  also
                agent.Start();
                return agent.IsRunning();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Exception is: {0}", e);
                return false;
            }
        }

        /// <summary>
        /// Stops the PaletteInsightAgentAgent service.
        /// </summary>
        /// <param name="hostControl">Service HostControl object</param>
        /// <returns>Indicator that service succesfully stopped</returns>
        public bool Stop(HostControl hostControl)
        {
            if (agent != null)
            {
                agent.Stop();
                agent.Dispose();
            }
            return true;
        }

        #endregion

        #region IDisposable Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                agent.Dispose();
            }
            disposed = true;
        }

        #endregion
    }
}