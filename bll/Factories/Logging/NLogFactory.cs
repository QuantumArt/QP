using System;
using System.Reflection;
using Quantumart.QP8.BLL.Adapters.Logging;
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
            return new NLogLogger((Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Name);
        }

        public ILog GetLogger(Type type)
        {
            return new NLogLogger(type);
        }

        public ILog GetLogger(string typeName)
        {
            return new NLogLogger(typeName);
        }
    }
}
