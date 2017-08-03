using System;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.Factories
{
    /// <summary>
    /// Provider to create logger from selected factory
    /// </summary>
    public class LogProvider
    {
        private static ILogFactory _logFactory;

        /// <summary>
        /// Gets or sets the log factory
        /// Use this to override the factory that is used to create loggers
        /// </summary>
        public static ILogFactory LogFactory
        {
            get => _logFactory ?? new DiagnosticsLogFactory();
            set => _logFactory = value;
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public static ILog GetLogger() => LogFactory.GetLogger();

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        public static ILog GetLogger(Type type) => LogFactory.GetLogger(type);

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        public static ILog GetLogger(string loggerName) => LogFactory.GetLogger(loggerName);
    }
}
