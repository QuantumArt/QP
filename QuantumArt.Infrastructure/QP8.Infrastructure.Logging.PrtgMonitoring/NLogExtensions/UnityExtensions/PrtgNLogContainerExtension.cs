using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.UnityExtensions
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
            Container.RegisterType<ILogFactory, PrtgNlogFactory>(new ContainerControlledLifetimeManager(), new InjectionConstructor(
                _prtgServiceStateVariableName,
                _prtgServiceQueueVariableName,
                _prtgServiceStatusVariableName));

            Container.RegisterType<ILog>(string.IsNullOrWhiteSpace(_loggerName)
                ? new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger(_loggerName)));

            LogProvider.LogFactory = Container.Resolve<ILogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
