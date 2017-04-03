using NLog.Common;
using NLog.Config;
using NLog.Targets;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions
{
    [Target("PrtgMonitoring")]
    public sealed class PrtgMonitoringTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public string IdentificationToken { get; set; }

        public IPrtgMonitoringService GetPrtgMonitoringService => new PrtgMonitoringService(Host, IdentificationToken);

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent.LogEvent);
            GetPrtgMonitoringService.SendStringPostRequest(logMessage);
            base.Write(logEvent);
        }
    }
}
