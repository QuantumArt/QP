using System;
using NLog;
using QP8.Infrastructure.Logging.Adapters;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Adapters
{
    /// <summary>
    /// Wrapper over the NLog extended with PrtgNLog monitoring
    /// </summary>
    public class PrtgNLogLogger : NLogLogger, IPrtgServiceLogger
    {
        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;
        private readonly LogLevel _defaultLogLevel = LogLevel.Info;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNLogLogger"/> class
        /// </summary>
        /// <param name="logger">NLog logger instanse</param>
        /// <param name="prtgServiceStateVariableName">The name of state parameter to send to prtg monitor</param>
        /// <param name="prtgServiceQueueVariableName">The name of queue parameter to send to prtg monitor</param>
        /// <param name="prtgServiceStatusVariableName">The name of status parameter to send to prtg monitor</param>
        public PrtgNLogLogger(ILogger logger, string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
            : base(logger)
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
