using Topshelf;
using SplunkNLog;
using NLog.Config;

namespace PaletteInsightAgentService
{
    /// <summary>
    /// Service layer that wraps the PaletteInsightAgentLib.  Topshelf keeps this pretty thin for us.
    /// </summary>
    internal class PaletteInsightAgentService
    {
        // Service name & description.
        private const string ServiceName = "PaletteInsightAgent";
        private const string ServiceDisplayname = "Palette Insight Agent";
        private const string ServiceDescription = "Performance monitoring for Tableau Server ";

        // Service recovery attempt timings, in minutes.
        private const int RecoveryFirstAttempt = 0;
        private const int RecoverySecondAttempt = 1;
        private const int RecoveryThirdAttempt = 5;

        // Service recovery reset period, in days.
        private const int RecoveryResetPeriod = 1;

        /// <summary>
        /// Main entry point for the service layer.
        /// </summary>
        public static void Main()
        {
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("SplunkNLog", typeof(SplunkNLog.SplunkNLog));


            // configure service parameters
            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.SetServiceName(ServiceName);
                hostConfigurator.SetDescription(ServiceDescription);
                hostConfigurator.SetDisplayName(ServiceDisplayname);
                hostConfigurator.Service(() => new PaletteInsightAgentServiceBootstrapper());
                hostConfigurator.RunAsLocalSystem();
                hostConfigurator.StartAutomaticallyDelayed();
                hostConfigurator.UseNLog();

                hostConfigurator.EnableServiceRecovery(r =>
                {
                    r.RestartService(RecoveryFirstAttempt);
                    r.RestartService(RecoverySecondAttempt);
                    r.RestartService(RecoveryThirdAttempt);
                    r.OnCrashOnly();
                    r.SetResetPeriod(RecoveryResetPeriod);
                });
            });
        }
    }
}