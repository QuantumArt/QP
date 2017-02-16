using System;
using Quantumart.QP8.BLL.Adapters.Logging;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories.Logging
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
