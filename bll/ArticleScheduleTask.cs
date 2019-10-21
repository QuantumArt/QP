using System;

namespace Quantumart.QP8.BLL
{
    public class ArticleScheduleTask
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public int FreqType { get; set; }

        public int FreqInterval { get; set; }

        public int FreqRelativeInterval { get; set; }

        public int FreqRecurrenceFactor { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public TimeSpan EndTime { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
