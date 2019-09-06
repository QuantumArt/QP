using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using DayOfWeek = Quantumart.QP8.Constants.DayOfWeek;

namespace Quantumart.QP8.BLL
{
    public class RecurringSchedule
    {
        public RecurringSchedule Clone() => (RecurringSchedule)MemberwiseClone();

        public static RecurringSchedule Empty
        {
            get
            {
                var result = new RecurringSchedule
                {
                    RepetitionStartDate = DateTime.Now.Date,
                    RepetitionNoEnd = true,
                    ScheduleRecurringType = ScheduleRecurringType.Daily,
                    ScheduleRecurringValue = 1,
                    DaySpecifyingType = DaySpecifyingType.Date,
                    Month = 0,
                    DayOfMonth = 1,
                    ShowStartTime = TimeSpan.Zero,
                    ShowLimitationType = ShowLimitationType.EndTime,
                    DurationValue = 12,
                    DurationUnit = ShowDurationUnit.Hours
                };

                result.RepetitionEndDate = result.GetDefaultRepetitionEndDate();
                result.ShowEndTime = result.GetDefaultShowEndTime();

                return result;
            }
        }

        public void DoCustomBinding()
        {
            if (RepetitionNoEnd)
            {
                RepetitionEndDate = GetDefaultRepetitionEndDate();
            }

            if (ShowLimitationType != ShowLimitationType.EndTime)
            {
                ShowEndTime = GetDefaultShowEndTime();
            }

        }

        public DateTime GetDefaultRepetitionEndDate() => RepetitionStartDate.Date.AddYears(1);

        public TimeSpan GetDefaultShowEndTime() => new TimeSpan(23, 59, 59);

        [Display(Name = "RepetitionStartDate", ResourceType = typeof(ArticleStrings))]
        public DateTime RepetitionStartDate { get; set; }

        [Display(Name = "RepetitionNoEnd", ResourceType = typeof(ArticleStrings))]
        public bool RepetitionNoEnd { get; set; }

        [Display(Name = "RepetitionEndDate", ResourceType = typeof(ArticleStrings))]
        public DateTime RepetitionEndDate { get; set; }

        [Display(Name = "ScheduleRecurringValue", ResourceType = typeof(ArticleStrings))]
        public int ScheduleRecurringValue { get; set; }

        [Display(Name = "ScheduleRecurringType", ResourceType = typeof(ArticleStrings))]
        public ScheduleRecurringType ScheduleRecurringType { get; set; }

        #region Week Days

        [Display(Name = "Monday", ResourceType = typeof(ArticleStrings))]
        public bool OnMonday { get; set; }

        [Display(Name = "Tuesday", ResourceType = typeof(ArticleStrings))]
        public bool OnTuesday { get; set; }

        [Display(Name = "Wednesday", ResourceType = typeof(ArticleStrings))]
        public bool OnWednesday { get; set; }

        [Display(Name = "Thursday", ResourceType = typeof(ArticleStrings))]
        public bool OnThursday { get; set; }

        [Display(Name = "Friday", ResourceType = typeof(ArticleStrings))]
        public bool OnFriday { get; set; }

        [Display(Name = "Saturday", ResourceType = typeof(ArticleStrings))]
        public bool OnSaturday { get; set; }

        [Display(Name = "Sunday", ResourceType = typeof(ArticleStrings))]
        public bool OnSunday { get; set; }

        #endregion

        [Display(Name = "DaySpecifyingType", ResourceType = typeof(ArticleStrings))]
        public DaySpecifyingType DaySpecifyingType { get; set; }

        [Display(Name = "MonthNumber", ResourceType = typeof(ArticleStrings))]
        public int Month { get; set; }

        [Display(Name = "DayOfMonth", ResourceType = typeof(ArticleStrings))]
        public int DayOfMonth { get; set; }

        [Display(Name = "WeekOfMonth", ResourceType = typeof(ArticleStrings))]
        public WeekOfMonth WeekOfMonth { get; set; }

        [Display(Name = "DayOfWeek", ResourceType = typeof(ArticleStrings))]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "ShowStartTime", ResourceType = typeof(ArticleStrings))]
        public TimeSpan ShowStartTime { get; set; }

        [Display(Name = "ShowEndTime", ResourceType = typeof(ArticleStrings))]
        public TimeSpan ShowEndTime { get; set; }

        [Display(Name = "ShowIsLimitedBy", ResourceType = typeof(ArticleStrings))]
        public ShowLimitationType ShowLimitationType { get; set; }

        [Display(Name = "DurationValue", ResourceType = typeof(ArticleStrings))]
        public int DurationValue { get; set; }

        [Display(Name = "DurationUnit", ResourceType = typeof(ArticleStrings))]
        public ShowDurationUnit DurationUnit { get; set; }
    }
}
