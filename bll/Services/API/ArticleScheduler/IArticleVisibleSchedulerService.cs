using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
	/// <summary>
	/// Интерфейс сервиса для управления показом статей по расписанию
	/// </summary>
	public interface IArticleRecurringSchedulerService
	{		
		/// <summary>
		/// Сделать статью видимой
		/// </summary>
		/// <param name="articleId"></param>
		Article ShowArticle(int articleId);
		/// <summary>
		/// Сделать статью невидимой
		/// </summary>
		/// <param name="articleId"></param>
		Article HideArticle(int articleId);
		/// <summary>
		/// Скрыть статью и удалить задачу (расписание)
		/// </summary>
		/// <param name="?"></param>
		Article HideAndCloseSchedule(int scheduleId);
		/// <summary>
		/// Получить текущее время на сервере БД
		/// </summary>
		/// <returns></returns>
		DateTime GetCurrentDBDateTime(); 
	}
}
