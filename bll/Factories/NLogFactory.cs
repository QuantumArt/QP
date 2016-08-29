using System;
using Quantumart.QP8.BLL.Adapters;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Factories
{
    public class NLogFactory : ILogFactory
    {
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
