using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
	/// <summary>
	/// Выполняет публикацию статьи по расписанию
	/// </summary>
	internal class PublishingTaskScheduler
	{
		private IArticlePublishingSchedulerService bllService;
		private IOperationsLogWriter operationsLogWriter;

		public PublishingTaskScheduler(IArticlePublishingSchedulerService bllService, IOperationsLogWriter operationsLogWriter)
		{
			if (bllService == null)
				throw new ArgumentNullException("bllService");
			if (operationsLogWriter == null)
				throw new ArgumentNullException("operationsLogWriter");

			this.bllService = bllService;
			this.operationsLogWriter = operationsLogWriter;
		}

		public void Run(PublishingTask task)
		{
			var currentTime = bllService.GetCurrentDBDateTime();
			if (currentTime >= task.PublishingDateTime)
			{
				var article = bllService.PublishAndCloseSchedule(task.Id);
				operationsLogWriter.PublishArticle(article);
			}
		}
	}
}
