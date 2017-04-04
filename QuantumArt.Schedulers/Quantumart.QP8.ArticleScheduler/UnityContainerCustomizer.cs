using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.UnityExtensions;
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
            .RegisterType<IOperationsLogWriter, OperationsLogWriter>()
            .AddNewExtension<NLogContainerExtension>();
        }

        public static IUnityContainer UnityContainer { get; }
    }
}
