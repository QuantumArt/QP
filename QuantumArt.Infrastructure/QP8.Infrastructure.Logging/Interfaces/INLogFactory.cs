using NLog;

namespace QP8.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// Factory to create NLog based ILog instances
    /// </summary>
    public interface INLogFactory : ILogFactory
    {
        /// <summary>
        /// Force logger to use new configuration file
        /// </summary>
        /// <param name="configPath">The logger configuration file</param>
        void ReloadConfiguration(string configPath);

        /// <summary>
        /// Create a new instance of the inner <see cref="ILogger"/> class
        /// </summary>
        /// <param name="loggerName">The string based logger name</param>
        ILogger CreateInnerLogger(string loggerName);
    }
}
