﻿using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.BLL.UnityExtensions;

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
            .RegisterType<IOperationsLogWriter, OperationsLogWriter>()
            .AddNewExtension<NLogContainerExtension>();
        }

        public static IUnityContainer UnityContainer { get; }
    }
}
