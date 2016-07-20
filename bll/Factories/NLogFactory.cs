using System;
using Quantumart.QP8.BLL.Adapters;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories
{
    /// <summary>
    /// ILogFactory that creates an NLog ILog logger
    /// </summary>
    public class NLogFactory : ILogFactory
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        public ILog GetLogger(Type type)
        {
            return new NLogLogger(type);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public ILog GetLogger(string typeName)
        {
            return new NLogLogger(typeName);
        }
    }
}
