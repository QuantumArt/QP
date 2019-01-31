using System;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleScheduleTaskMapper : GenericMapper<ArticleScheduleTask, ArticleScheduleDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ArticleScheduleDAL, ArticleScheduleTask>(MemberList.Source)
                .ForMember(data => data.Duration, opt => opt.Ignore())
                .AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(ArticleScheduleDAL dataObject, ArticleScheduleTask task)
        {
            task.StartDate = ScheduleHelper.GetScheduleDateFromSqlValues(dataObject.ActiveStartDate, DateTime.Now.AddDays(1));
            task.StartTime = ScheduleHelper.GetScheduleTimeFromSqlValues(dataObject.ActiveStartTime, TimeSpan.MinValue);
            task.EndDate = ScheduleHelper.GetScheduleDateFromSqlValues(dataObject.ActiveEndDate, DateTime.Now.AddDays(1));
            task.EndTime = ScheduleHelper.GetScheduleTimeFromSqlValues(dataObject.ActiveEndTime, TimeSpan.MinValue);
            task.Duration = dataObject.UseDuration == 1 ? ScheduleHelper.GetDuration(dataObject.DurationUnits, dataObject.Duration, task.StartDate) : task.EndTime - task.StartTime;
        }
    }
}
