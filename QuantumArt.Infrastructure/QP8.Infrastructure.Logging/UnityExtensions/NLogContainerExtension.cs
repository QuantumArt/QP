using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.UnityExtensions
{
    public class NLogContainerExtension : UnityContainerExtension
    {
        private readonly string _loggerName;

        public NLogContainerExtension()
        {
        }

        public NLogContainerExtension(string loggerName)
        {
            _loggerName = loggerName;
        }

        protected override void Initialize()
        {
            Container.RegisterType<ILogFactory, NLogFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILog>(string.IsNullOrWhiteSpace(_loggerName)
                ? new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger(_loggerName)));

            LogProvider.LogFactory = Container.Resolve<ILogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
