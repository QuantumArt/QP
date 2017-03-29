using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler
{
    internal static class UnityContainerCustomizer
    {
        static UnityContainerCustomizer()
        {
            UnityContainer = new UnityContainer()
                .RegisterType<IArticleSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticleOnetimeSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticlePublishingSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticleRecurringSchedulerService, ArticleSchedulerService>()
                .RegisterType<IExceptionHandler, ExceptionHandler>()
                .RegisterType<IOperationsLogWriter, OperationsLogWriter>();
            // TODO: Uncomment when update Unity to 4.0 .AddNewExtension<NLogContainerExtension>();

            UnityContainer.RegisterType<ILogFactory, NLogFactory>(new ContainerControlledLifetimeManager());
            UnityContainer.RegisterType<ILog>(new InjectionFactory(c => c.Resolve<ILogFactory>().GetLogger()));
            LogProvider.LogFactory = UnityContainer.Resolve<ILogFactory>();
            Logger.Log = UnityContainer.Resolve<ILog>();
        }

        public static IUnityContainer UnityContainer { get; }
    }
}
