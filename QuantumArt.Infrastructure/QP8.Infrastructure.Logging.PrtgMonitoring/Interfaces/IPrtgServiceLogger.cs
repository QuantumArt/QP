using System;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces
{
    public interface IPrtgServiceLogger : ILog
    {
        void LogMessage(PrtgServiceMonitoringMessage message);

        void LogMessage(PrtgServiceMonitoringMessage message, Exception ex);
    }
}
