using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;


namespace WebMvc.Test.BLL
{
	[TestClass]
	public class ArticleScheduleTest
	{
		[TestMethod]
		public void CopyFromTest()
		{
			Article srcArticle = new Article();
			ArticleSchedule source = new ArticleSchedule
			{
				Article = srcArticle,
				ArticleId = 10,
				Id = 100,

				EndDate = DateTime.Now.AddDays(-1),
				PublicationDate = DateTime.Now.AddDays(-2),
				ScheduleType = C.ScheduleTypeEnum.Recurring,
				StartDate = DateTime.Now.AddDays(-3),
				StartRightNow = true,
				WithoutEndDate = true,

				Recurring = new RecurringSchedule
				{
					DayOfMonth = 1,
					DayOfWeek = C.DayOfWeek.Wednesday,
					DaySpecifyingType = C.DaySpecifyingType.DayOfWeek,
					DurationUnit = C.ShowDurationUnit.Months,
					DurationValue = 2,
					Month = 3,
					OnMonday = true,
					RepetitionEndDate = DateTime.Now.AddDays(1),
					RepetitionNoEnd = true,
					RepetitionStartDate = DateTime.Now.AddDays(2),
					ScheduleRecurringType = C.ScheduleRecurringType.Monthly,
					ScheduleRecurringValue = 3,
					ShowEndTime = TimeSpan.FromMinutes(22),
					ShowLimitationType = C.ShowLimitationType.Duration,
					ShowStartTime = TimeSpan.FromMinutes(122),
					WeekOfMonth = C.WeekOfMonth.LastWeek
				}
			};

			ArticleSchedule dest = new ArticleSchedule
			{
				Article = new Article(),
				ArticleId = 20,
				Id = 200,

				Recurring = RecurringSchedule.Empty
			};


			dest.CopyFrom(source);

			Assert.ReferenceEquals(dest.Article, srcArticle);
			Assert.AreEqual(dest.ArticleId, 20);
			Assert.AreEqual(dest.Id, 200);
			
			Assert.AreEqual(source.EndDate, dest.EndDate);
			Assert.AreEqual(source.IsVisible, dest.IsVisible);
			Assert.AreEqual(source.PublicationDate, dest.PublicationDate);
			Assert.AreEqual(source.ScheduleType, dest.ScheduleType);
			Assert.AreEqual(source.StartDate, dest.StartDate);
			Assert.AreEqual(source.StartRightNow, dest.StartRightNow);
			Assert.AreEqual(source.WithoutEndDate, dest.WithoutEndDate);
			
			Assert.AreEqual(source.Recurring.DayOfMonth, dest.Recurring.DayOfMonth);
			Assert.AreEqual(source.Recurring.DayOfWeek, dest.Recurring.DayOfWeek);
			Assert.AreEqual(source.Recurring.DaySpecifyingType, dest.Recurring.DaySpecifyingType);
			Assert.AreEqual(source.Recurring.DurationUnit, dest.Recurring.DurationUnit);
			Assert.AreEqual(source.Recurring.DurationValue, dest.Recurring.DurationValue);
			Assert.AreEqual(source.Recurring.Month, dest.Recurring.Month);
			Assert.AreEqual(source.Recurring.OnFriday, dest.Recurring.OnFriday);
			Assert.AreEqual(source.Recurring.OnMonday, dest.Recurring.OnMonday);
			Assert.AreEqual(source.Recurring.OnSaturday, dest.Recurring.OnSaturday);
			Assert.AreEqual(source.Recurring.OnSunday, dest.Recurring.OnSunday);
			Assert.AreEqual(source.Recurring.OnThursday, dest.Recurring.OnThursday);
			Assert.AreEqual(source.Recurring.OnTuesday, dest.Recurring.OnTuesday);
			Assert.AreEqual(source.Recurring.OnWednesday, dest.Recurring.OnWednesday);
			Assert.AreEqual(source.Recurring.RepetitionEndDate, dest.Recurring.RepetitionEndDate);
			Assert.AreEqual(source.Recurring.RepetitionNoEnd, dest.Recurring.RepetitionNoEnd);
			Assert.AreEqual(source.Recurring.RepetitionStartDate, dest.Recurring.RepetitionStartDate);
			Assert.AreEqual(source.Recurring.ScheduleRecurringType, dest.Recurring.ScheduleRecurringType);
			Assert.AreEqual(source.Recurring.ScheduleRecurringValue, dest.Recurring.ScheduleRecurringValue);
			Assert.AreEqual(source.Recurring.ShowEndTime, dest.Recurring.ShowEndTime);
			Assert.AreEqual(source.Recurring.ShowLimitationType, dest.Recurring.ShowLimitationType);
			Assert.AreEqual(source.Recurring.ShowStartTime, dest.Recurring.ShowStartTime);
			Assert.AreEqual(source.Recurring.WeekOfMonth, dest.Recurring.WeekOfMonth);					
		}
	}
}
