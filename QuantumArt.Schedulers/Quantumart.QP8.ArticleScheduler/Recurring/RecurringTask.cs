using System;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    internal class RecurringTask : ISchedulerTask
    {
        public RecurringTask(
            int id,
            int articleId,
            RecurringTaskTypes taskType,
            int interval,
            int relativeInterval,
            int recurrenceFactor,
            DateTime startDate,
            DateTime endDate,
            TimeSpan startTime,
            TimeSpan duration)
        {
            Id = id;
            ArticleId = articleId;
            TaskType = taskType;
            Interval = interval;
            RelativeInterval = relativeInterval;
            RecurrenceFactor = recurrenceFactor;
            StartDate = startDate;
            EndDate = endDate;
            StartTime = startTime;
            Duration = duration;
        }

        public int Id { get; }

        public int ArticleId { get; }

        public RecurringTaskTypes TaskType { get; }

        public int Interval { get; }

        public int RelativeInterval { get; }

        public int RecurrenceFactor { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan Duration { get; }

        public static RecurringTask Create(ArticleScheduleTask task) => new RecurringTask(
            task.Id,
            task.ArticleId,
            (RecurringTaskTypes)task.FreqType,
            task.FreqInterval,
            task.FreqRelativeInterval,
            task.FreqRecurrenceFactor,
            task.StartDate,
            task.EndDate,
            task.StartTime,
            task.Duration);
    }
}
