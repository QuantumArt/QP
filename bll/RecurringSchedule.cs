using System;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
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
					RepetitionEndDate = DateTime.Now.Date.AddYears(1),
					ScheduleRecurringType = ScheduleRecurringType.Daily,
					ScheduleRecurringValue = 1,
					DaySpecifyingType = DaySpecifyingType.Date,
					Month = 0,
					DayOfMonth = 1,
					ShowStartTime = TimeSpan.Zero,
					ShowEndTime = new TimeSpan(23, 59, 59),
					ShowLimitationType = ShowLimitationType.EndTime,
					DurationValue = 12,
					DurationUnit = ShowDurationUnit.Hours					
				};

				result.RepetitionEndDate = result.GetDefaultRepetitionEndDate();
				return result;
			}
		}	
		
		public DateTime GetDefaultRepetitionEndDate() => RepetitionStartDate.Date.AddYears(1);

	    [LocalizedDisplayName("RepetitionStartDate", NameResourceType = typeof(ArticleStrings))]
		public DateTime RepetitionStartDate { get; set; }

		[LocalizedDisplayName("RepetitionNoEnd", NameResourceType = typeof(ArticleStrings))]
		public bool RepetitionNoEnd { get; set; }

		[LocalizedDisplayName("RepetitionEndDate", NameResourceType = typeof(ArticleStrings))]
		public DateTime RepetitionEndDate { get; set; }

		[LocalizedDisplayName("ScheduleRecurringValue", NameResourceType = typeof(ArticleStrings))]
		public int ScheduleRecurringValue { get; set; }

		[LocalizedDisplayName("ScheduleRecurringType", NameResourceType = typeof(ArticleStrings))]
		public ScheduleRecurringType ScheduleRecurringType { get; set; }

		#region Week Days
		[LocalizedDisplayName("Monday", NameResourceType = typeof(ArticleStrings))]
		public bool OnMonday { get; set; }

		[LocalizedDisplayName("Tuesday", NameResourceType = typeof(ArticleStrings))]
		public bool OnTuesday { get; set; }

		[LocalizedDisplayName("Wednesday", NameResourceType = typeof(ArticleStrings))]
		public bool OnWednesday { get; set; }

		[LocalizedDisplayName("Thursday", NameResourceType = typeof(ArticleStrings))]
		public bool OnThursday { get; set; }

		[LocalizedDisplayName("Friday", NameResourceType = typeof(ArticleStrings))]
		public bool OnFriday { get; set; }

		[LocalizedDisplayName("Saturday", NameResourceType = typeof(ArticleStrings))]
		public bool OnSaturday { get; set; }

		[LocalizedDisplayName("Sunday", NameResourceType = typeof(ArticleStrings))]
		public bool OnSunday { get; set; }
		#endregion

		[LocalizedDisplayName("DaySpecifyingType", NameResourceType = typeof(ArticleStrings))]
		public DaySpecifyingType DaySpecifyingType { get; set; }

		[LocalizedDisplayName("MonthNumber", NameResourceType = typeof(ArticleStrings))]
		public int Month { get; set; }

		[LocalizedDisplayName("DayOfMonth", NameResourceType = typeof(ArticleStrings))]
		public int DayOfMonth { get; set; }

		[LocalizedDisplayName("WeekOfMonth", NameResourceType = typeof(ArticleStrings))]
		public WeekOfMonth WeekOfMonth { get; set; }

		[LocalizedDisplayName("DayOfWeek", NameResourceType = typeof(ArticleStrings))]
		public DayOfWeek DayOfWeek { get; set; }

		[LocalizedDisplayName("ShowStartTime", NameResourceType = typeof(ArticleStrings))]
		public TimeSpan ShowStartTime { get; set; }

		[LocalizedDisplayName("ShowEndTime", NameResourceType = typeof(ArticleStrings))]
		public TimeSpan ShowEndTime { get; set; }

		[LocalizedDisplayName("ShowIsLimitedBy", NameResourceType = typeof(ArticleStrings))]
		public ShowLimitationType ShowLimitationType { get; set; }

		[LocalizedDisplayName("DurationValue", NameResourceType = typeof(ArticleStrings))]
		public int DurationValue { get; set; }

		[LocalizedDisplayName("DurationUnit", NameResourceType = typeof(ArticleStrings))]
		public ShowDurationUnit DurationUnit { get; set; }		
	}
}
