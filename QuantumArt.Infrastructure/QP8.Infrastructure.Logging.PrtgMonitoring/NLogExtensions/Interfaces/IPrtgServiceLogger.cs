using System;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces
{
    public interface IPrtgServiceLogger : ILog
    {
        void LogMessage(PrtgServiceMonitoringMessage message);

        void LogMessage(PrtgServiceMonitoringMessage message, Exception ex);
    }
}
