using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
	/// <summary>
	/// Выполняет onetime-задачу
	/// </summary>
	internal class OnetimeTaskScheduler
	{
		private IArticleOnetimeSchedulerService bllService;
		private IOperationsLogWriter operationsLogWriter;

		public OnetimeTaskScheduler(IArticleOnetimeSchedulerService bllService, IOperationsLogWriter operationsLogWriter)
		{
			if (bllService == null)
				throw new ArgumentNullException("bllService");
			if (operationsLogWriter == null)
				throw new ArgumentNullException("operationsLogWriter");

			this.bllService = bllService;
			this.operationsLogWriter = operationsLogWriter;
		}

		/// <summary>
		/// Выполнить задачу
		/// </summary>
		/// <param name="task"></param>
		public void Run(OnetimeTask task)
		{
			var range = Tuple.Create(task.StartDateTime, task.EndDateTime);
			var currentTime = bllService.GetCurrentDBDateTime();
			var pos = range.Position(currentTime);
			Article article = null;
			if (pos < 0) // до диапазона задачи
				return;
			else if (pos > 0) // после диапазона задачи
			{
				article = bllService.HideAndCloseSchedule(task.Id);
				if (article != null && article.Visible)
					operationsLogWriter.HideArticle(article);
			}
			else if (pos == 0) // в диапазоне
			{
				if (task.EndDateTime.Year == ArticleScheduleConstants.Infinity.Year) // конец диапазона в "бесконечности"
					article = bllService.ShowAndCloseSchedule(task.Id);
				else
					article = bllService.ShowArticle(task.ArticleId);
				if (article != null && !article.Visible)
					operationsLogWriter.ShowArticle(article);
			}

		}
	}
}
