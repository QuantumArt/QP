using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Targets
{
    [Target("PrtgMonitoring")]
    public sealed class PrtgMonitoringTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string Host { get; set; }

        [RequiredParameter]
        public Layout IdentificationToken { get; set; }

        public IPrtgMonitoringService GetPrtgMonitoringService(AsyncLogEventInfo logEvent) =>
            new PrtgMonitoringService(Host, IdentificationToken.Render(logEvent.LogEvent));

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent.LogEvent);
            GetPrtgMonitoringService(logEvent).SendStringPostRequest(logMessage);
            base.Write(logEvent);
        }
    }
}
