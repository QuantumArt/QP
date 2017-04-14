using System;

namespace QP8.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// Factory to create ILog instances
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Gets the logger
        /// </summary>
        ILog GetLogger();

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        ILog GetLogger(Type type);

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        ILog GetLogger(string loggerName);
    }
}
