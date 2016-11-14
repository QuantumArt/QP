using System;

namespace Quantumart.QP8.BLL.Services.ArticleScheduler
{
    public interface IArticlePublishingSchedulerService
    {
        /// <summary>
        /// Опубликовать статью и удалить задачу (расписание)
        /// </summary>
        Article PublishAndCloseSchedule(int scheduleId);

        /// <summary>
        /// Получить текущее время на сервере БД
        /// </summary>
        DateTime GetCurrentDBDateTime();
    }
}
