using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
	public interface IArticlePublishingSchedulerService
	{
		/// <summary>
		/// Опубликовать статью и удалить задачу (расписание)
		/// </summary>
		/// <param name="articleId"></param>
		/// <param name="scheduleId"></param>
		Article PublishAndCloseSchedule(int scheduleId);
		/// <summary>
		/// Получить текущее время на сервере БД
		/// </summary>
		/// <returns></returns>
		DateTime GetCurrentDBDateTime(); 
	}
}
