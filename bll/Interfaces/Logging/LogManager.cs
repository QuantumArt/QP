using System;

namespace Quantumart.QP8.BLL.Interfaces.Logging
{
    /// <summary>
    /// Manager to create logger from selected factory
    /// </summary>
    public class LogManager
    {
        private static ILogFactory _logFactory;

        /// <summary>
        /// Gets or sets the log factory.
        /// Use this to override the factory that is used to create loggers
        /// </summary>
        public static ILogFactory LogFactory
        {
            get
            {
                return _logFactory ?? new NullLogFactory();
            }
            set { _logFactory = value; }
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog GetLogger(Type type)
        {
            return LogFactory.GetLogger(type);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog GetLogger(string typeName)
        {
            return LogFactory.GetLogger(typeName);
        }
    }
}
