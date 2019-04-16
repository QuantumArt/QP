using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ScheduleRepository
    {
        internal static ArticleSchedule GetSchedule(Article item)
        {
            var dal = GetDalByArticleId(item.Id);
            return dal == null ? ArticleSchedule.CreateSchedule(item) : MapperFacade.ArticleScheduleMapper.GetBizObject(dal, item);
        }

        internal static ArticleSchedule GetScheduleById(int id)
        {
            var dal = GetDalById(id);
            return dal == null ? null : MapperFacade.ArticleScheduleMapper.GetBizObject(dal);
        }

        internal static void UpdateSchedule(Article article)
        {
            var item = article.Schedule;
            if (item != null)
            {
                item.Article = article;
                item.ArticleId = article.Id;

                var originalId = item.Id;
                item.Id = 0;

                var dalItem = MapperFacade.ArticleScheduleMapper.GetDalObject(item);
                var itemPersisted = originalId != 0;
                var hasChanges = !itemPersisted;

                if (itemPersisted)
                {
                    var originalItem = GetDalById(originalId);
                    hasChanges = DetectChanges(originalItem, dalItem);
                    if (hasChanges)
                    {
                        DefaultRepository.Delete<ArticleScheduleDAL>(originalId);
                    }
                }

                var needToPersist = dalItem.FreqType != ScheduleFreqTypes.None && hasChanges;
                if (needToPersist)
                {
                    dalItem.UseService = QPConfiguration.UseScheduleService;
                    dalItem.Modified = article.Modified;
                    dalItem.LastModifiedBy = article.LastModifiedBy;
                    DefaultRepository.SimpleSave(dalItem);
                }
            }
        }

        private static bool DetectChanges(ArticleScheduleDAL originalItem, ArticleScheduleDAL dalItem) => originalItem.ActiveEndDate != dalItem.ActiveEndDate ||
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

        internal static ArticleScheduleDAL GetDalByArticleId(int id)
        {
            return QPContext.EFContext.ArticleScheduleSet.SingleOrDefault(n => n.ArticleId == id);
        }

        internal static ArticleScheduleDAL GetDalById(int id)
        {
            return QPContext.EFContext.ArticleScheduleSet.SingleOrDefault(n => n.Id == id);
        }

        internal static void Delete(int id)
        {
            DefaultRepository.Delete<ArticleScheduleDAL>(id);
        }

        internal static void Delete(ArticleSchedule schedule)
        {
            if (schedule != null)
            {
                Delete(schedule.Id);
            }
        }

        public static IEnumerable<ArticleScheduleTask> GetScheduleTaskList()
        {
            return MapperFacade.ArticleScheduleTaskMapper.GetBizList(QPContext.EFContext.ArticleScheduleSet.Where(t => !t.Deactivate && t.UseService).ToList());
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
