using System;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
    public class ArticleSchedule
    {
        public static ArticleSchedule CreateSchedule(Article item)
        {
            ArticleSchedule result = new ArticleSchedule();
            result.ScheduleType = item.Visible ? ScheduleTypeEnum.Visible : ScheduleTypeEnum.Invisible;
            result.ArticleId = item.Id;
            result.Article = item;
            result.StartDate = ScheduleHelper.DefaultStartDate;
            result.EndDate = ScheduleHelper.DefaultEndDate;
            result.PublicationDate = ScheduleHelper.DefaultStartDate;
            result.WithoutEndDate = true;

            result.Recurring = RecurringSchedule.Empty;
            return result;
        }

        internal void CopyFrom(ArticleSchedule schedule)
        {
            EndDate = schedule.EndDate;
            PublicationDate = schedule.PublicationDate;
            ScheduleType = schedule.ScheduleType;
            StartDate = schedule.StartDate;
            StartRightNow = schedule.StartRightNow;
            WithoutEndDate = schedule.WithoutEndDate;
            Recurring = schedule.Recurring.Clone();
        }

        public Article Article;

        public int ArticleId { get; set; }

        public int Id { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(ArticleStrings))]
        public DateTime StartDate { get; set; }

        [LocalizedDisplayName("EndDate", NameResourceType = typeof(ArticleStrings))]
        public DateTime EndDate { get; set; }

        [LocalizedDisplayName("StartRightNow", NameResourceType = typeof(ArticleStrings))]
        public bool StartRightNow { get; set; }

        [LocalizedDisplayName("WithoutEndDate", NameResourceType = typeof(ArticleStrings))]
        public bool WithoutEndDate { get; set; }

        [LocalizedDisplayName("ScheduleType", NameResourceType = typeof(ArticleStrings))]
        public ScheduleTypeEnum ScheduleType { get; set; }

        [LocalizedDisplayName("PublicationTime", NameResourceType = typeof(ArticleStrings))]
        public DateTime PublicationDate { get; set; }

        public RecurringSchedule Recurring { get; set; }

        public bool IsVisible
        {
            get
            {
                return ScheduleType == ScheduleTypeEnum.Visible || ScheduleType == ScheduleTypeEnum.OneTimeEvent && StartRightNow;
            }
        }
    }
}
