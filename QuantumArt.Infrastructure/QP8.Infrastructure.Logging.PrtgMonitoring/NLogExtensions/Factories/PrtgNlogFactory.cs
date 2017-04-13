using System;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Adapters;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories
{
    /// <summary>
    /// Creates a Prtg logger based on nlog, that logs all messages to nlog targets and add support for prtg monitoring
    /// </summary>
    public class PrtgNlogFactory : ILogFactory
    {
        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;

        public PrtgNlogFactory(string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        public ILog GetLogger()
        {
            return GetLogger(AssemblyHelpers.GetAssemblyName());
        }

        public ILog GetLogger(Type type)
        {
            return type == null ? GetLogger() : new PrtgNLogLogger(type, _prtgServiceStateVariableName, _prtgServiceQueueVariableName, _prtgServiceStatusVariableName);
        }

        public ILog GetLogger(string loggerName)
        {
            return string.IsNullOrWhiteSpace(loggerName) ? GetLogger() : new PrtgNLogLogger(loggerName, _prtgServiceStateVariableName, _prtgServiceQueueVariableName, _prtgServiceStatusVariableName);
        }
    }
}
