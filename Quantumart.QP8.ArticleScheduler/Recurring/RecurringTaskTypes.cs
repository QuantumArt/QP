using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
	/// <summary>
	/// Типы расписаний циклических задач 
	/// </summary>
	public enum RecurringTaskTypes : int
	{
		Daily = ScheduleFreqTypes.RecurringDaily,
		Weekly = ScheduleFreqTypes.RecurringWeekly,
		Monthly = ScheduleFreqTypes.RecurringMonthly,
		MonthlyRelative = ScheduleFreqTypes.RecurringMonthlyRelative
	}
}
