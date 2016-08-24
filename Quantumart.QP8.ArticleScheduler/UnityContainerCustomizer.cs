using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler
{
	/// <summary>
	/// Настраивает UnityContainer
	/// </summary>
	static class UnityContainerCustomizer
	{
		static IUnityContainer unityContainer = null;

		static UnityContainerCustomizer()
		{
			unityContainer = new UnityContainer()
			.RegisterType<IArticleSchedulerService, ArticleSchedulerService>()
			.RegisterType<IArticleOnetimeSchedulerService, ArticleSchedulerService>()
			.RegisterType<IArticlePublishingSchedulerService, ArticleSchedulerService>()
			.RegisterType<IArticleRecurringSchedulerService, ArticleSchedulerService>()
			.RegisterType<IExceptionHandler, ExceptionHandler>()
			.RegisterType<IOperationsLogWriter, OperationsLogWriter>();
		}
		
		public static IUnityContainer UnityContainer  {get{return unityContainer;}}
	}
}
