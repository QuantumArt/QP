using System;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

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
            task.Duration = dataObject.UseDuration == 1
                ? ScheduleHelper.GetDuration(dataObject.DurationUnits, dataObject.Duration, task.StartDate)
                : task.EndDate.TimeOfDay - task.StartDate.TimeOfDay;
        }
    }
}
