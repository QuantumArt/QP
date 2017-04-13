using System;
using NLog;
using QP8.Infrastructure.Logging.Adapters;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Adapters
{
    public class PrtgNLogLogger : NLogLogger, IPrtgServiceLogger
    {
        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;
        private readonly LogLevel _defaultLogLevel = LogLevel.Info;

        public PrtgNLogLogger(string loggerName, string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
            : base(loggerName)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        public PrtgNLogLogger(Type type, string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
            : base(type)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        public void LogMessage(PrtgServiceMonitoringMessage message)
        {
            LogMessage(message, null);
        }

        public void LogMessage(PrtgServiceMonitoringMessage message, Exception ex)
        {
            var logEventInfo = new LogEventInfo(_defaultLogLevel, LoggerName, null, message.Message, null, ex);
            logEventInfo.Properties[_prtgServiceStateVariableName] = (int)message.State;
            logEventInfo.Properties[_prtgServiceQueueVariableName] = message.Queue;
            logEventInfo.Properties[_prtgServiceStatusVariableName] = message.Status;

            Logger.Log(logEventInfo);
        }
    }
}
