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
            Container.RegisterType<INLogFactory, NLogFactory>(new ContainerControlledLifetimeManager(), new InjectionConstructor());
            Container.RegisterType<ILog>(string.IsNullOrWhiteSpace(_loggerName)
                ? new InjectionFactory(c => c.Resolve<INLogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<INLogFactory>().GetLogger(_loggerName)));

            LogProvider.LogFactory = Container.Resolve<INLogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
