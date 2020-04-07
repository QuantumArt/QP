using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ArticleSchedule
    {
        public static ArticleSchedule CreateSchedule(Article item)
        {
            var result = new ArticleSchedule();
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

        internal void DoCustomBinding()
        {
            if (ScheduleType != ScheduleTypeEnum.Recurring)
            {
                Recurring = RecurringSchedule.Empty;
            }
            else
            {
                Recurring.DoCustomBinding();
            }

            if (ScheduleType != ScheduleTypeEnum.OneTimeEvent)
            {
                StartDate = ScheduleHelper.DefaultStartDate;
                EndDate = ScheduleHelper.DefaultEndDate;
            }
            else
            {
                if (StartRightNow || StartDate < DateTime.Now)
                {
                    StartDate = DateTime.Now.AddSeconds(10);
                }

                if (WithoutEndDate)
                {
                    EndDate = ArticleScheduleConstants.Infinity;
                }
            }

            if (StartRightNow && WithoutEndDate)
            {
                ScheduleType = ScheduleTypeEnum.Visible;
            }

            if (Article.Delayed)
            {
                StartDate = PublicationDate;
                EndDate = ArticleScheduleConstants.Infinity;
            }

        }

        public Article Article;

        public int ArticleId { get; set; }

        public int Id { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(ArticleStrings))]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(ArticleStrings))]
        public DateTime EndDate { get; set; }

        [Display(Name = "StartRightNow", ResourceType = typeof(ArticleStrings))]
        public bool StartRightNow { get; set; }

        [Display(Name = "WithoutEndDate", ResourceType = typeof(ArticleStrings))]
        public bool WithoutEndDate { get; set; }

        [Display(Name = "ScheduleType", ResourceType = typeof(ArticleStrings))]
        public ScheduleTypeEnum ScheduleType { get; set; }

        [Display(Name = "PublicationTime", ResourceType = typeof(ArticleStrings))]
        public DateTime PublicationDate { get; set; }

        public RecurringSchedule Recurring { get; set; }

        public bool IsVisible => ScheduleType == ScheduleTypeEnum.Visible || ScheduleType == ScheduleTypeEnum.OneTimeEvent && StartRightNow;
    }
}
