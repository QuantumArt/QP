using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Factories.Logging;
using Quantumart.QP8.BLL.Interfaces.Logging;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.BLL.UnityExtensions
{
    public class NLogContainerExtension : UnityContainerExtension
    {
        public static string LoggerTypeName = null;

        protected override void Initialize()
        {
            Container.RegisterType<ILogFactory, NLogFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILog>(new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger(LoggerTypeName)));
            LogProvider.LogFactory = Container.Resolve<ILogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
