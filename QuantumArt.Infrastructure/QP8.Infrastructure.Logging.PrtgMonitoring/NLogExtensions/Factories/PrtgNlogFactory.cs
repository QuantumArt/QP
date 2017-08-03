using System;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Adapters;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories
{
    /// <summary>
    /// Creates a Prtg logger based on nlog, that logs all messages to nlog targets and add support for prtg monitoring
    /// </summary>
    public class PrtgNLogFactory : NLogFactory, IPrtgNLogFactory
    {
        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNLogLogger" /> class
        /// </summary>
        /// <param name="prtgServiceStateVariableName">The name of state parameter to send to prtg monitor</param>
        /// <param name="prtgServiceQueueVariableName">The name of queue parameter to send to prtg monitor</param>
        /// <param name="prtgServiceStatusVariableName">The name of status parameter to send to prtg monitor</param>
        public PrtgNLogFactory(string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNLogLogger" /> class
        /// </summary>
        /// <param name="configPath">Path to NLog config file</param>
        /// <param name="prtgServiceStateVariableName">The name of state parameter to send to prtg monitor</param>
        /// <param name="prtgServiceQueueVariableName">The name of queue parameter to send to prtg monitor</param>
        /// <param name="prtgServiceStatusVariableName">The name of status parameter to send to prtg monitor</param>
        public PrtgNLogFactory(string configPath, string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
            : base(configPath)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public new IPrtgServiceLogger GetLogger() => (IPrtgServiceLogger)base.GetLogger();

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        public new IPrtgServiceLogger GetLogger(Type type) => (IPrtgServiceLogger)base.GetLogger(type);

        /// <summary>
        /// Initializes a new instance of the <see cref="IPrtgServiceLogger" /> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        public new IPrtgServiceLogger GetLogger(string loggerName) => (IPrtgServiceLogger)base.GetLogger(loggerName);

        /// <summary>
        /// Create a new instance of the <see cref="PrtgNLogLogger" /> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        protected override ILog CreateLogger(string loggerName) => new PrtgNLogLogger(
            CreateInnerLogger(loggerName),
            _prtgServiceStateVariableName,
            _prtgServiceQueueVariableName,
            _prtgServiceStatusVariableName
        );
    }
}
