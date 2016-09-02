using System;
using System.Reflection;
using Quantumart.QP8.BLL.Adapters.Logging;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories.Logging
{
    /// <summary>
    /// Creates a NLog Logger
    /// </summary>
    public class NLogFactory : ILogFactory
    {
        public ILog GetLogger()
        {
            return new NLogLogger(Assembly.GetEntryAssembly().GetName().Name);
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
