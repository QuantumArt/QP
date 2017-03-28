using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging.UnityExtensions
{
    public class NLogContainerExtension : UnityContainerExtension
    {
        public static string LoggerTypeName = null;

        protected override void Initialize()
        {
            Container.RegisterType<ILogFactory, NLogFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILog>(string.IsNullOrWhiteSpace(LoggerTypeName)
                ? new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger(LoggerTypeName)));

            LogProvider.LogFactory = Container.Resolve<ILogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
