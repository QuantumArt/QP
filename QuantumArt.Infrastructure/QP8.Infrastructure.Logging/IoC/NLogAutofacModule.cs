using System;
using Autofac;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.IoC
{
    public class NLogAutofacModule : Module
    {
        private readonly string _configPath;
        private readonly string _loggerName;
        private readonly Type _type;

        public NLogAutofacModule()
        {
        }

        public NLogAutofacModule(string configPath, string loggerName)
        {
            _configPath = configPath;
            _loggerName = loggerName;
        }

        public NLogAutofacModule(string configPath, Type type)
        {
            _configPath = configPath;
            _type = type;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(RegisterNLogFactory).As<INLogFactory>().As<ILogFactory>();
            builder.Register(RegisterNLogInstance).As<ILog>();
        }

        private NLogFactory RegisterNLogFactory(IComponentContext cc)
        {
            var factory = string.IsNullOrWhiteSpace(_configPath) ? new NLogFactory() : new NLogFactory(_configPath);
            LogProvider.LogFactory = factory;
            return factory;
        }

        private ILog RegisterNLogInstance(IComponentContext cc)
        {
            var instanse = _type == null ? cc.Resolve<INLogFactory>().GetLogger(_loggerName) : cc.Resolve<INLogFactory>().GetLogger(_type);
            Logger.Log = instanse;
            return instanse;
        }
    }
}
