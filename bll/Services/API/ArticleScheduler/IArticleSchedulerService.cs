using System.Collections.Generic;

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
        IEnumerable<ArticleScheduleTask> GetScheduleTaskList();
    }
}
