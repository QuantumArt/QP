using System;

namespace Quantumart.QP8.BLL.Services.API.ArticleScheduler
{
    /// <summary>
    /// Интерфейс сервиса для управления показом статей по расписанию
    /// </summary>
    public interface IArticleRecurringSchedulerService
    {
        /// <summary>
        /// Сделать статью видимой
        /// </summary>
        Article ShowArticle(int articleId);

        /// <summary>
        /// Сделать статью невидимой
        /// </summary>
        Article HideArticle(int articleId);

        /// <summary>
        /// Скрыть статью и удалить задачу (расписание)
        /// </summary>
        Article HideAndCloseSchedule(int scheduleId);

        /// <summary>
        /// Получить текущее время на сервере БД
        /// </summary>
        DateTimeOffset GetCurrentDBDateTime();
    }
}
