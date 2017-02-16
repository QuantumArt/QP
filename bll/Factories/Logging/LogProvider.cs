using System;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories.Logging
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
            get
            {
                return _logFactory ?? new DiagnosticsLogFactory();
            }
            set
            {
                _logFactory = value;
            }
        }

        /// <summary>
        /// Gets the logger with calling assembly name
        /// </summary>
        public static ILog GetLogger()
        {
            return LogFactory.GetLogger();
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public static ILog GetLogger(Type type)
        {
            return LogFactory.GetLogger(type);
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public static ILog GetLogger(string typeName)
        {
            return LogFactory.GetLogger(typeName);
        }
    }
}
