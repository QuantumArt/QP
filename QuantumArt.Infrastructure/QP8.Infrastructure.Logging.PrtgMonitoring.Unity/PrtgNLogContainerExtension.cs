using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.Unity
{
    public class PrtgNLogContainerExtension : UnityContainerExtension
    {
        private readonly string _loggerName;
        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;

        public PrtgNLogContainerExtension(string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
        {
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        public PrtgNLogContainerExtension(string loggerName, string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
        {
            _loggerName = loggerName;
            _prtgServiceStateVariableName = prtgServiceStateVariableName;
            _prtgServiceQueueVariableName = prtgServiceQueueVariableName;
            _prtgServiceStatusVariableName = prtgServiceStatusVariableName;
        }

        protected override void Initialize()
        {
            Container.RegisterType<IPrtgNLogFactory, PrtgNLogFactory>(new ContainerControlledLifetimeManager(), new InjectionConstructor(
                _prtgServiceStateVariableName,
                _prtgServiceQueueVariableName,
                _prtgServiceStatusVariableName));

            Container.RegisterType<IPrtgServiceLogger>(new ContainerControlledLifetimeManager(), string.IsNullOrWhiteSpace(_loggerName)
                ? new InjectionFactory(c => c.Resolve<IPrtgNLogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<IPrtgNLogFactory>().GetLogger(_loggerName)));

            LogProvider.LogFactory = Container.Resolve<IPrtgNLogFactory>();
            Logger.Log = Container.Resolve<IPrtgServiceLogger>();
        }
    }
}
