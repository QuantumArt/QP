using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    public class OnetimeTask
    {
        public static OnetimeTask Create(ArticleScheduleTask task)
        {
            return new OnetimeTask(task.Id, task.ArticleId, task.StartDate + task.StartTime, task.EndDate + task.EndTime);
        }

        public OnetimeTask(int id, int articleId, DateTime startDateTime, DateTime endDateTime)
        {
            Id = id;
            ArticleId = articleId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public int Id { get; }

        public int ArticleId { get; }

        public DateTime StartDateTime { get; }

        public DateTime EndDateTime { get; }
    }
}
