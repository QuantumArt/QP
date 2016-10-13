using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ScheduleRepository
    {

        internal static ArticleSchedule GetSchedule(Article item)
        {
            ArticleScheduleDAL dal = GetDalByArticleId(item.Id);
            if (dal == null)
				return ArticleSchedule.CreateSchedule(item);
            else
            {
                return MapperFacade.ArticleScheduleMapper.GetBizObject(dal, item);
            }
        }

		internal static ArticleSchedule GetScheduleById(int id)
		{
			ArticleScheduleDAL dal = GetDalById(id);
			if (dal == null)
				return null;
			else
				return MapperFacade.ArticleScheduleMapper.GetBizObject(dal);
		}

        internal static void UpdateSchedule(Article article)
        {
            ArticleSchedule item = article.Schedule;

            if (item != null)
            {
                item.Article = article;
                item.ArticleId = article.Id;

                int originalId = item.Id;
                item.Id = 0;
                ArticleScheduleDAL dalItem = MapperFacade.ArticleScheduleMapper.GetDalObject(item);

                bool itemPersisted = (originalId != 0);
                bool hasChanges = !itemPersisted;


                if (itemPersisted)
                {
                    ArticleScheduleDAL originalItem = GetDalById(originalId);//QPContext.CurrentDBContext.ArticleScheduleSet.SingleOrDefault(s => s.Id == (decimal)originalId);//
                    hasChanges = DetectChanges(originalItem, dalItem);
                    if (hasChanges)
                    {
                        DefaultRepository.Delete<ArticleScheduleDAL>(originalId);
                    }
                }

				bool needToPersist = (dalItem.FreqType != ScheduleFreqTypes.None) && hasChanges;
                if (needToPersist)
                {
					dalItem.UseService = QPConfiguration.UseScheduleService;
					dalItem.Modified = article.Modified;
					dalItem.LastModifiedBy = article.LastModifiedBy;
					DefaultRepository.SimpleSave<ArticleScheduleDAL>(dalItem);
                }
            }
        }

        private static bool DetectChanges(ArticleScheduleDAL originalItem, ArticleScheduleDAL dalItem)
        {
            return
                originalItem.ActiveEndDate != dalItem.ActiveEndDate ||
                originalItem.ActiveEndTime != dalItem.ActiveEndTime ||
                originalItem.ActiveStartTime != dalItem.ActiveStartTime ||
                originalItem.ActiveStartDate != dalItem.ActiveStartDate ||
                originalItem.Duration != dalItem.Duration ||
                originalItem.DurationUnits != dalItem.DurationUnits ||
                originalItem.UseDuration != dalItem.UseDuration ||
                originalItem.Occurences != dalItem.Occurences ||
                originalItem.MaximumOccurences != dalItem.MaximumOccurences ||
                originalItem.FreqType != dalItem.FreqType ||
                originalItem.FreqSubdayType != dalItem.FreqSubdayType ||
                originalItem.FreqSubdayInterval != dalItem.FreqSubdayInterval ||
                originalItem.FreqRelativeInterval != dalItem.FreqRelativeInterval ||
                originalItem.FreqRecurrenceFactor != dalItem.FreqRecurrenceFactor ||
                originalItem.FreqInterval != dalItem.FreqInterval;
        }

        internal static ArticleScheduleDAL GetDalByArticleId(int id)
        {
            return QPContext.EFContext.ArticleScheduleSet.SingleOrDefault(n => n.ArticleId == (decimal)id);
        }

        internal static ArticleScheduleDAL GetDalById(int id)
        {
            return QPContext.EFContext
				.ArticleScheduleSet
				.SingleOrDefault(n => n.Id == (decimal)id);
        }

		/// <summary>
		/// Удаляет расписание
		/// </summary>
		/// <param name="id"></param>
		internal static void Delete(int id)
		{
			DefaultRepository.Delete<ArticleScheduleDAL>(id);
		}

		/// <summary>
		/// Удаляет расписание
		/// </summary>
		/// <param name="id"></param>
		internal static void Delete(ArticleSchedule schedule)
		{
			if (schedule != null)
				Delete(schedule.Id);
		}

		/// <summary>
		/// Получает список раписаний для обработки сервисом расписаний
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<ArticleScheduleTask> GetScheduleTaskList()
		{
			// TODO: добавить условие по USE_SERVICE == true
			return MapperFacade.ArticleScheduleTaskMapper.GetBizList(QPContext.EFContext.ArticleScheduleSet
				.Where(t => !t.Deactivate && t.UseService)
				.ToList());
		}

        internal static void CopyScheduleToChildDelays(Article item)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateChildDelayedSchedule(scope.DbConnection, item.Id);
            }
           
            

        }
    }
}
