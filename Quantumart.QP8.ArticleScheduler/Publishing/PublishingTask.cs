using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    public class PublishingTask
    {
        /// <summary>
        /// Создать PublishingTask из ArticleScheduleTask
        /// </summary>
        /// <param name="task"></param>
        public static PublishingTask Create(ArticleScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.FreqType != 2)
            {
                throw new ArgumentException("Undefined FreqType value: " + task.FreqType);
            }

            return new PublishingTask(task.Id, task.ArticleId, task.StartDate + task.StartTime);
        }

        public PublishingTask(int id, int articleId, DateTime publishingDateTime)
        {
            Id = id;
            ArticleId = articleId;
            PublishingDateTime = publishingDateTime;
        }

        public int Id { get; private set; }

        public int ArticleId { get; private set; }

        public DateTime PublishingDateTime { get; private set; }
    }
}
