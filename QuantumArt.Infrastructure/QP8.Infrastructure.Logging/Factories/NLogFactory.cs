using System;
using NLog;
using NLog.Config;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Adapters;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.Factories
{
    /// <summary>
    /// Factory for creating an NLog logger instances
    /// </summary>
    public class NLogFactory : INLogFactory
    {
        protected readonly LogFactory LogFactory;

        /// <summary>
        /// Creates a NLog logger factory, that search config at default places
        /// </summary>
        public NLogFactory()
        {
            LogFactory = new LogFactory();
        }

        /// <summary>
        /// Creates a NLog logger factory, that search config at specified path
        /// </summary>
        /// <param name="configPath">Path to NLog config file</param>
        public NLogFactory(string configPath)
        {
            LogFactory = new LogFactory();
            LogFactory.Configuration = new XmlLoggingConfiguration(configPath, LogFactory);
        }

        /// <summary>
        /// Use full type name if creating logger by type name
        /// </summary>
        public static bool UseFullTypeNames { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class
        /// </summary>
        public virtual ILog GetLogger()
        {
            return GetLogger(AssemblyHelpers.GetAssemblyName());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        public virtual ILog GetLogger(Type type)
        {
            return type == null
                ? GetLogger()
                : GetLogger(UseFullTypeNames ? type.FullName : type.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        public virtual ILog GetLogger(string loggerName)
        {
            return string.IsNullOrWhiteSpace(loggerName)
                ? GetLogger()
                : CreateLogger(loggerName);
        }

        /// <summary>
        /// Flushing all messages and init new configuration config
        /// </summary>
        /// <param name="configPath">Path to nlog config</param>
        public virtual void ReloadConfiguration(string configPath)
        {
            LogFactory.Flush();
            LogFactory.Configuration = new XmlLoggingConfiguration(configPath, LogFactory);
            LogFactory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Create a new instance of the <see cref="NLog.Logger"/> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        public virtual ILogger CreateInnerLogger(string loggerName)
        {
            Ensure.Argument.NotNullOrWhiteSpace(loggerName);
            return LogFactory.GetLogger(loggerName);
        }

        /// <summary>
        /// Create a new instance of the <see cref="NLogLogger"/> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        protected virtual ILog CreateLogger(string loggerName)
        {
            Ensure.Argument.NotNullOrWhiteSpace(loggerName);
            return new NLogLogger(CreateInnerLogger(loggerName));
        }
    }
}
