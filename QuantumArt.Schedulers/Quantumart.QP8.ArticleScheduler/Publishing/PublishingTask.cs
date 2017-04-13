using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    public class PublishingTask
    {
        public static PublishingTask Create(ArticleScheduleTask task)
        {
            return new PublishingTask(task.Id, task.ArticleId, task.StartDate + task.StartTime);
        }

        public PublishingTask(int id, int articleId, DateTime publishingDateTime)
        {
            Id = id;
            ArticleId = articleId;
            PublishingDateTime = publishingDateTime;
        }

        public int Id { get; }

        public int ArticleId { get; }

        public DateTime PublishingDateTime { get; }
    }
}
