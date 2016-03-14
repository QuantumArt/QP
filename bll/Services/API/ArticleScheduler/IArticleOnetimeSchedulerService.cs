using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
	public interface IArticleOnetimeSchedulerService
	{
		/// <summary>
		/// Сделать статью видимой
		/// </summary>
		/// <param name="articleId"></param>
		Article ShowArticle(int articleId);		
		/// <summary>
		/// Скрыть статью и удалить задачу (расписание)
		/// </summary>
		/// <param name="?"></param>
		Article HideAndCloseSchedule(int scheduleId);
		/// <summary>
		/// Показать статью и удалить задачу (расписание)
		/// </summary>
		/// <param name="?"></param>
		Article ShowAndCloseSchedule(int scheduleId);
		/// <summary>
		/// Получить текущее время на сервере БД
		/// </summary>
		/// <returns></returns>
		DateTime GetCurrentDBDateTime(); 
	}
}
