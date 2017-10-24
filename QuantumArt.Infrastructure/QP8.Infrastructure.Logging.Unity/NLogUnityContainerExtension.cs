using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace QP8.Infrastructure.Logging.Unity
{
    public class NLogUnityContainerExtension : UnityContainerExtension
    {
        private readonly string _loggerName;

        public NLogUnityContainerExtension()
        {
        }

        public NLogUnityContainerExtension(string loggerName)
        {
            _loggerName = loggerName;
        }

        protected override void Initialize()
        {
            Container.RegisterType<INLogFactory, NLogFactory>(new ContainerControlledLifetimeManager(), new InjectionConstructor());
            Container.RegisterType<ILog>(new ContainerControlledLifetimeManager(), string.IsNullOrWhiteSpace(_loggerName)
                ? new InjectionFactory(c => c.Resolve<INLogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<INLogFactory>().GetLogger(_loggerName)));

            LogProvider.LogFactory = Container.Resolve<INLogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
