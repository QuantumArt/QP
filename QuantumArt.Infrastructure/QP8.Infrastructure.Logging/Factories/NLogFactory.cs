using System;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Adapters;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.Factories
{
    /// <summary>
    /// Creates a NLog logger, that logs all messages to nlog targets
    /// </summary>
    public class NLogFactory : ILogFactory
    {
        public ILog GetLogger()
        {
            return GetLogger(AssemblyHelpers.GetAssemblyName());
        }

        public ILog GetLogger(Type type)
        {
            return type == null ? GetLogger() : new NLogLogger(type);
        }

        public ILog GetLogger(string typeName)
        {
            return string.IsNullOrWhiteSpace(typeName) ? GetLogger() : new NLogLogger(typeName);
        }
    }
}
