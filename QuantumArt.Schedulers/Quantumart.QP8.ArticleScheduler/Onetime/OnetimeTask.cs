using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    public class OnetimeTask : ISchedulerTask
    {
        public OnetimeTask(int id, int articleId, DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            Id = id;
            ArticleId = articleId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public int Id { get; }

        public int ArticleId { get; }

        public DateTimeOffset StartDateTime { get; }

        public DateTimeOffset EndDateTime { get; }

        public static OnetimeTask CreateOnetimeTask(ArticleScheduleTask task) => new OnetimeTask(task.Id, task.ArticleId, task.StartDate + task.StartTime, task.EndDate + task.EndTime);
    }
}
