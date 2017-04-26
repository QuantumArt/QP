using System;

namespace QP8.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// Factory to create ILog instances
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ILog"/> class
        /// </summary>
        ILog GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ILog"/> class
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        ILog GetLogger(Type type);

        /// <summary>
        /// Initializes a new instance of the <see cref="ILog"/> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        ILog GetLogger(string loggerName);
    }
}
