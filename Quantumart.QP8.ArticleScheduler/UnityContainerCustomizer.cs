using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler
{
    /// <summary>
    /// Настраивает UnityContainer
    /// </summary>
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
        }

        public static IUnityContainer UnityContainer { get; }
    }
}
