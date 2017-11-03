using QP8.Infrastructure.Logging.PrtgMonitoring.Unity;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Constants;
using Unity;

namespace Quantumart.QP8.ArticleScheduler
{
    internal class UnityContainerCustomizer
    {
        public UnityContainerCustomizer()
        {
            UnityContainer = new UnityContainer()
                .RegisterType<DbScheduler>()
                .RegisterType<IOnetimeTaskScheduler, OnetimeTaskScheduler>()
                .RegisterType<IRecurringTaskScheduler, RecurringTaskScheduler>()
                .RegisterType<IPublishingTaskScheduler, PublishingTaskScheduler>()
                .RegisterType<IArticleSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticleOnetimeSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticlePublishingSchedulerService, ArticleSchedulerService>()
                .RegisterType<IArticleRecurringSchedulerService, ArticleSchedulerService>()
                .AddExtension(new PrtgNLogContainerExtension(
                    LoggerData.DefaultPrtgServiceStateVariableName,
                    LoggerData.DefaultPrtgServiceQueueVariableName,
                    LoggerData.DefaultPrtgServiceStatusVariableName)
                );
        }

        public IUnityContainer UnityContainer { get; }
    }
}
