using System;

namespace Quantumart.QP8.BLL.Services.API.ArticleScheduler
{
    public interface IArticleOnetimeSchedulerService
    {
        /// <summary>
        /// Сделать статью видимой
        /// </summary>
        Article ShowArticle(int articleId);

        /// <summary>
        /// Скрыть статью и удалить задачу (расписание)
        /// </summary>
        Article HideAndCloseSchedule(int scheduleId);

        /// <summary>
        /// Показать статью и удалить задачу (расписание)
        /// </summary>
        Article ShowAndCloseSchedule(int scheduleId);

        /// <summary>
        /// Получить текущее время на сервере БД
        /// </summary>
        DateTime GetCurrentDBDateTime();
    }
}
