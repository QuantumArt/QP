using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    public class OnetimeTask
    {
        public static OnetimeTask Create(ArticleScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.FreqType != 1)
            {
                throw new ArgumentException("Undefined FreqType value: " + task.FreqType);
            }

            return new OnetimeTask(task.Id, task.ArticleId, task.StartDate + task.StartTime, task.EndDate + task.EndTime);
        }

        public OnetimeTask(int id, int articleId, DateTime startDateTime, DateTime endDateTime)
        {
            Id = id;
            ArticleId = articleId;

            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public int Id { get; private set; }

        public int ArticleId { get; private set; }

        public DateTime StartDateTime { get; private set; }

        public DateTime EndDateTime { get; private set; }
    }
}
