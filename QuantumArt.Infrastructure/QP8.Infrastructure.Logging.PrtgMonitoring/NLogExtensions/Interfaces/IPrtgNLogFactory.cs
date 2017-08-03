using System;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces
{
    /// <summary>
    /// Factory to create NLog based ILog instances extended with prtg monitoring support
    /// </summary>
    public interface IPrtgNLogFactory : INLogFactory
    {
        /// <summary>
        /// Gets the logger
        /// </summary>
        new IPrtgServiceLogger GetLogger();

        /// <summary>
        /// Gets the logger
        /// </summary>
        /// <param name="type">The type on which logger name is based</param>
        new IPrtgServiceLogger GetLogger(Type type);

        /// <summary>
        /// Initializes a new instance of the <see cref="IPrtgServiceLogger" /> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        new IPrtgServiceLogger GetLogger(string loggerName);
    }
}
