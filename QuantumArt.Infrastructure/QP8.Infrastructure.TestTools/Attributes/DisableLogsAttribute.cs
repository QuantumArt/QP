using System;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.TestTools.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DisableLogsAttribute : Attribute
    {
        private readonly ILogFactory _logFactory;

        public DisableLogsAttribute()
        {
            _logFactory = LogProvider.LogFactory;
            LogProvider.LogFactory = new NullLogFactory();
        }

        ~DisableLogsAttribute()
        {
            LogProvider.LogFactory = _logFactory;
        }
    }
}
