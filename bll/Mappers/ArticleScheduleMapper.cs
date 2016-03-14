using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleScheduleMapper : GenericMapper<ArticleSchedule, ArticleScheduleDAL>
    {

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<ArticleSchedule, ArticleScheduleDAL>()
                //.ForMember(data => data.MaximumOccurences, opt => opt.MapFrom(src => Utils.Converter.ToNullableDecimal(src.MaximumOccurences)))
                .ForMember(data => data.Article, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ArticleScheduleDAL, ArticleSchedule>()
                .ForMember(data => data.Article, opt => opt.Ignore())
				.ForMember(data => data.Recurring, opt => opt.Ignore());
        }


        public ArticleSchedule GetBizObject(ArticleScheduleDAL dataObject, Article item)
        {
            ArticleSchedule result = GetBizObject(dataObject);
            result.Article = item;
            ArticleScheduleMapper.ProceedComplexMappingToBiz(dataObject, result);
            return result;
        }

        public override ArticleScheduleDAL GetDalObject(ArticleSchedule bizObject)
        {
            ArticleScheduleDAL result = base.GetDalObject(bizObject);
            ArticleScheduleMapper.ProceedComplexMappingToDal(bizObject, result);
            return result;
        }

        private static void ProceedComplexMappingToDal(ArticleSchedule item, ArticleScheduleDAL result)
        {
            result.FreqType = GetFreqType(item);

			if (result.FreqType == ScheduleFreqTypes.OneTime || result.FreqType == ScheduleFreqTypes.Publishing)
			{
				ProceedOneTimeScheduleToDal(item, result);
			}
			else if (result.FreqType >= ScheduleFreqTypes.RecurringDaily && result.FreqType <= ScheduleFreqTypes.RecurringMonthlyRelative)
			{
				ProceedRecurringScheduleToDal(item.Recurring, result);
			}
        }

        private static void ProceedComplexMappingToBiz(ArticleScheduleDAL dal, ArticleSchedule result)
		{
			result.ScheduleType = ParseFreqType(result.Article.Visible, dal.FreqType);			

			if (result.ScheduleType == ScheduleTypeEnum.Recurring)
				result.Recurring = ProceedDalToRecurringSchedule(dal);
			else
				result.Recurring = RecurringSchedule.Empty;
			
			ProceedDalToOneTimeSchedule(dal, result);
			result.PublicationDate = (result.Article.Delayed) ? result.StartDate : ScheduleHelper.DefaultStartDate;
		}


		private static void ProceedDalToOneTimeSchedule(ArticleScheduleDAL dal, ArticleSchedule result)
		{
			result.StartDate = ScheduleHelper.GetStartDateTime(dal.ActiveStartDate, dal.ActiveStartTime);
			result.EndDate = ScheduleHelper.GetEndDateTime(dal.ActiveEndDate, dal.ActiveEndTime);
			if (result.EndDate.Year == ArticleScheduleConstants.Infinity.Year)
			{
				result.EndDate = ScheduleHelper.DefaultEndDate;
				result.WithoutEndDate = true;
			}
			else
				result.WithoutEndDate = false;
		}

		private static void ProceedOneTimeScheduleToDal(ArticleSchedule item, ArticleScheduleDAL result)
		{
			Tuple<int, int> startValues = ScheduleHelper.GetSqlValuesFromScheduleDateTime(item.StartDate);
			result.ActiveStartDate = startValues.Item1;
			result.ActiveStartTime = startValues.Item2;
			Tuple<int, int> endValues = ScheduleHelper.GetSqlValuesFromScheduleDateTime(item.EndDate);
			result.ActiveEndDate = endValues.Item1;
			result.ActiveEndTime = endValues.Item2;
			result.Duration = 1M;
			result.DurationUnits = "dd";
		}

		/// <summary>
		/// Преобразовать данные из БД в RecurringSchedule 
		/// </summary>
		/// <param name="dal"></param>
		/// <returns></returns>
		private static RecurringSchedule ProceedDalToRecurringSchedule(ArticleScheduleDAL dal)
		{
			var result = RecurringSchedule.Empty;

			// Интервал повторений
			result.RepetitionStartDate = ScheduleHelper.GetScheduleDateFromSqlValues(dal.ActiveStartDate, DateTime.Now);
			result.RepetitionEndDate = ScheduleHelper.GetScheduleDateFromSqlValues(dal.ActiveEndDate, result.GetDefaultRepetitionEndDate());
			if (result.RepetitionEndDate.Year == ArticleScheduleConstants.Infinity.Year)
			{
				result.RepetitionEndDate = result.GetDefaultRepetitionEndDate();
				result.RepetitionNoEnd = true;
			}
			else
				result.RepetitionNoEnd = false;
				

			// Определение дней показа			
			if (dal.FreqType == ScheduleFreqTypes.RecurringDaily) // день
			{
				result.ScheduleRecurringType = ScheduleRecurringType.Daily;
				result.ScheduleRecurringValue = dal.FreqInterval;
			}
			else if (dal.FreqType == ScheduleFreqTypes.RecurringWeekly) // неделя
			{
				result.ScheduleRecurringType = ScheduleRecurringType.Weekly;
				result.ScheduleRecurringValue = dal.FreqRecurrenceFactor;
				result.OnSunday = (dal.FreqInterval & 1) == 1;
				result.OnMonday = (dal.FreqInterval & 2) == 2;
				result.OnTuesday = (dal.FreqInterval & 4) == 4;
				result.OnWednesday = (dal.FreqInterval & 8) == 8;
				result.OnThursday = (dal.FreqInterval & 16) == 16;
				result.OnFriday = (dal.FreqInterval & 32) == 32;
				result.OnSaturday = (dal.FreqInterval & 64) == 64;
			}
			else if (dal.FreqType == ScheduleFreqTypes.RecurringMonthly || dal.FreqType == ScheduleFreqTypes.RecurringMonthlyRelative) // Month или MonthRelative
			{
				if (dal.FreqRecurrenceFactor % 12 == 0) // Год
				{
					result.ScheduleRecurringType = ScheduleRecurringType.Yearly;
					result.ScheduleRecurringValue = dal.FreqRecurrenceFactor / 12;
					result.Month = result.RepetitionStartDate.Month;
				}
				else // Месяц
				{
					result.ScheduleRecurringType = ScheduleRecurringType.Monthly;
					result.ScheduleRecurringValue = dal.FreqRecurrenceFactor;
				}

				if (dal.FreqType == ScheduleFreqTypes.RecurringMonthly) // Month
				{
					result.DaySpecifyingType = DaySpecifyingType.Date;
					result.DayOfMonth = dal.FreqInterval;
				}
				else if (dal.FreqType == ScheduleFreqTypes.RecurringMonthlyRelative) // MonthRelative
				{
					result.DaySpecifyingType = DaySpecifyingType.DayOfWeek;					
					result.WeekOfMonth = (WeekOfMonth)dal.FreqRelativeInterval;
					result.DayOfWeek = (Constants.DayOfWeek)dal.FreqInterval;
				}								
			}
			

			// Интервал времени показа
			result.ShowStartTime = ScheduleHelper.GetScheduleTimeFromSqlValues(dal.ActiveStartTime);
			if (dal.UseDuration == 0)
			{
				result.ShowLimitationType = ShowLimitationType.EndTime;				
				result.ShowEndTime = ScheduleHelper.GetScheduleTimeFromSqlValues(dal.ActiveEndTime);
			}
			else 
			{
				result.ShowLimitationType = ShowLimitationType.Duration;
				result.DurationValue = Convert.ToInt32(dal.Duration);
				result.DurationUnit = ScheduleHelper.ParseDurationUnit(dal.DurationUnits);
			}
			
			return result;
		}

		private static void ProceedRecurringScheduleToDal(RecurringSchedule item, ArticleScheduleDAL result)
		{			
			result.ActiveStartDate = ScheduleHelper.GetSqlValuesFromScheduleDate(item.RepetitionStartDate);
			result.ActiveStartTime = ScheduleHelper.GetSqlValuesFromScheduleTime(item.ShowStartTime);

			var repetitionEndDate = item.RepetitionNoEnd ? ArticleScheduleConstants.Infinity.Date : item.RepetitionEndDate;
			var repetitionEndTime = item.ShowLimitationType == ShowLimitationType.Duration ? ArticleScheduleConstants.Infinity.TimeOfDay : item.ShowEndTime;
			result.ActiveEndDate = ScheduleHelper.GetSqlValuesFromScheduleDate(repetitionEndDate);
			result.ActiveEndTime = ScheduleHelper.GetSqlValuesFromScheduleTime(repetitionEndTime);

			result.UseDuration = item.ShowLimitationType == ShowLimitationType.Duration ? 1 : 0;
			result.Duration = Convert.ToDecimal(item.DurationValue);
			result.DurationUnits = item.DurationUnit.ToDbValue();

			if (item.ScheduleRecurringType == ScheduleRecurringType.Daily)
			{				
				result.FreqInterval = item.ScheduleRecurringValue;
			}
			else if (item.ScheduleRecurringType == ScheduleRecurringType.Weekly)
			{				

				result.FreqRecurrenceFactor = item.ScheduleRecurringValue;

				result.FreqInterval = 0;
				if(item.OnSunday)
					result.FreqInterval = result.FreqInterval | 1;
				if (item.OnMonday)
					result.FreqInterval = result.FreqInterval | 2;
				if (item.OnTuesday)
					result.FreqInterval = result.FreqInterval | 4;
				if (item.OnWednesday)
					result.FreqInterval = result.FreqInterval | 8;
				if (item.OnThursday)
					result.FreqInterval = result.FreqInterval | 16;
				if (item.OnFriday)
					result.FreqInterval = result.FreqInterval | 32;
				if (item.OnSaturday)
					result.FreqInterval = result.FreqInterval | 64;					
			}
			else if (item.ScheduleRecurringType == ScheduleRecurringType.Monthly || item.ScheduleRecurringType == ScheduleRecurringType.Yearly)
			{
				if (item.ScheduleRecurringType == ScheduleRecurringType.Yearly)
				{
					result.FreqRecurrenceFactor = item.ScheduleRecurringValue  * 12;

					var dt = new DateTime(item.RepetitionStartDate.Year, item.Month, 1);
					if (dt < DateTime.Now.Date)
						dt = dt.AddYears(1);
					result.ActiveStartDate = ScheduleHelper.GetSqlValuesFromScheduleDate(dt);
				}
				else if (item.ScheduleRecurringType == ScheduleRecurringType.Monthly)
				{
					result.FreqRecurrenceFactor = item.ScheduleRecurringValue;
				}

				if (item.DaySpecifyingType == DaySpecifyingType.Date) // Month
				{
					result.FreqInterval = item.DayOfMonth;
				}
				else if (item.DaySpecifyingType == DaySpecifyingType.DayOfWeek) // MonthRelative
				{
					result.FreqRelativeInterval = (int)item.WeekOfMonth;
					result.FreqInterval = (int)item.DayOfWeek;
				}	
			}
			
		}

		/// <summary>
		/// Определить ScheduleType
		/// </summary>
		/// <param name="visible"></param>
		/// <param name="freqType"></param>
		/// <returns></returns>
		private static ScheduleTypeEnum ParseFreqType(bool visible, int freqType) // item.Visible
		{
			if (freqType == ScheduleFreqTypes.None || freqType == ScheduleFreqTypes.Publishing)
			{
				return visible ? ScheduleTypeEnum.Visible : ScheduleTypeEnum.Invisible;
			}
			else if (freqType == ScheduleFreqTypes.OneTime)
				return ScheduleTypeEnum.OneTimeEvent;
			else
				return ScheduleTypeEnum.Recurring;
		}

		/// <summary>
		/// Определить FreqType 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private static int GetFreqType(ArticleSchedule item)
		{
			if (item.Article.Delayed)
				return ScheduleFreqTypes.Publishing;
			else if (item.ScheduleType == ScheduleTypeEnum.Invisible || item.ScheduleType == ScheduleTypeEnum.Visible)
				return ScheduleFreqTypes.None;
			else if (item.ScheduleType == ScheduleTypeEnum.OneTimeEvent)
				return ScheduleFreqTypes.OneTime;
			else if (item.ScheduleType == ScheduleTypeEnum.Recurring)
			{
				if (item.Recurring.ScheduleRecurringType == ScheduleRecurringType.Daily)
					return ScheduleFreqTypes.RecurringDaily;
				else if (item.Recurring.ScheduleRecurringType == ScheduleRecurringType.Weekly)
					return ScheduleFreqTypes.RecurringWeekly;
				else if (item.Recurring.ScheduleRecurringType == ScheduleRecurringType.Monthly || item.Recurring.ScheduleRecurringType == ScheduleRecurringType.Yearly)
				{
					if (item.Recurring.DaySpecifyingType == DaySpecifyingType.Date)
						return ScheduleFreqTypes.RecurringMonthly;
					else if (item.Recurring.DaySpecifyingType == DaySpecifyingType.DayOfWeek)
						return ScheduleFreqTypes.RecurringMonthlyRelative;

					throw new ArgumentException("ScheduleRecurringType is undefined");
				}
			}

			throw new ArgumentException("ScheduleType is undefined");
		}        
    }
}
