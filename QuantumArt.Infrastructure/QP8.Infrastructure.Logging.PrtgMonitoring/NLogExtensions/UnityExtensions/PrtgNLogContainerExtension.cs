using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.UnityExtensions
{
    public class PrtgNLogContainerExtension : UnityContainerExtension
    {
        public static string LoggerTypeName = null;

        private readonly string _prtgServiceStateVariableName;
        private readonly string _prtgServiceQueueVariableName;
        private readonly string _prtgServiceStatusVariableName;

        public PrtgNLogContainerExtension(string prtgServiceStateVariableName, string prtgServiceQueueVariableName, string prtgServiceStatusVariableName)
        {
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

            Container.RegisterType<ILog>(string.IsNullOrWhiteSpace(LoggerTypeName)
                ? new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger())
                : new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger(LoggerTypeName)));

            LogProvider.LogFactory = Container.Resolve<ILogFactory>();
            Logger.Log = Container.Resolve<ILog>();
        }
    }
}
