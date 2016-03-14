using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
	/// <summary>
	/// Интерфейс сервиса для работы с расписаниями
	/// </summary>
	public interface IArticleSchedulerService
	{
		/// <summary>
		/// Получить список задач (расписаний)
		/// </summary>
		/// <returns></returns>
		IEnumerable<ArticleScheduleTask> GetScheduleTaskList();		
	}
}
