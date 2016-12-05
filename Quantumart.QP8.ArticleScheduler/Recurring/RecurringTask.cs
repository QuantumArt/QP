using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    /// <summary>
    /// Задача циклического расписания
    /// </summary>
    public class RecurringTask
    {
        /// <summary>
        /// Создать RecurringTask из ArticleScheduleTask
        /// </summary>
        public static RecurringTask Create(ArticleScheduleTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.FreqType != (int)RecurringTaskTypes.Daily &&
                task.FreqType != (int)RecurringTaskTypes.Monthly &&
                task.FreqType != (int)RecurringTaskTypes.MonthlyRelative &&
                task.FreqType != (int)RecurringTaskTypes.Weekly)
            {
                throw new ArgumentException("Undefined FreqType value: " + task.FreqType);
            }

            return new RecurringTask(task.Id, task.ArticleId, (RecurringTaskTypes)task.FreqType,
                task.FreqInterval, task.FreqRelativeInterval, task.FreqRecurrenceFactor,
                task.StartDate, task.EndDate, task.StartTime, task.Duration);
        }

        public RecurringTask(int id, int articleId, RecurringTaskTypes taskType, int interval, int relativeInterval, int recurrenceFactor, DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan duration)
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

        public int Id { get; private set; }

        public int ArticleId { get; private set; }

        public RecurringTaskTypes TaskType { get; private set; }

        public int Interval { get; private set; }

        public int RelativeInterval { get; private set; }

        public int RecurrenceFactor { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan Duration { get; private set; }
    }
}
