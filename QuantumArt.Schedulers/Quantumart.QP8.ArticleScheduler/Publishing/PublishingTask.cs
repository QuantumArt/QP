using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    public class PublishingTask : ISchedulerTask
    {
        public PublishingTask(int id, int articleId, DateTimeOffset publishingDateTime)
        {
            Id = id;
            ArticleId = articleId;
            PublishingDateTime = publishingDateTime;
        }

        public int Id { get; }

        public int ArticleId { get; }

        public DateTimeOffset PublishingDateTime { get; }

        public static PublishingTask Create(ArticleScheduleTask task)
        {
            return new PublishingTask(task.Id, task.ArticleId, task.StartDate + task.StartTime);
        }
    }
}
