using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
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
			this.EndDate = schedule.EndDate;			
			this.PublicationDate = schedule.PublicationDate;
			this.ScheduleType = schedule.ScheduleType;
			this.StartDate = schedule.StartDate;
			this.StartRightNow = schedule.StartRightNow;
			this.WithoutEndDate = schedule.WithoutEndDate;
			this.Recurring = schedule.Recurring.Clone();
		}

		public Article Article;
        public int ArticleId { get; set; }
        public int Id { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(ArticleStrings))]		
        public DateTime StartDate { get; set; }

        [LocalizedDisplayName("EndDate", NameResourceType = typeof(ArticleStrings))]
        public DateTime EndDate
        {
            get;
            set;
        }

        [LocalizedDisplayName("StartRightNow", NameResourceType = typeof(ArticleStrings))]
        public bool StartRightNow { get; set; }

        [LocalizedDisplayName("WithoutEndDate", NameResourceType = typeof(ArticleStrings))]
         public bool WithoutEndDate { get; set; }

        [LocalizedDisplayName("ScheduleType", NameResourceType = typeof(ArticleStrings))]
        public ScheduleTypeEnum ScheduleType
        {
            get;
            set;
        }

        [LocalizedDisplayName("PublicationTime", NameResourceType = typeof(ArticleStrings))]
        public DateTime PublicationDate { get; set; }


		public RecurringSchedule Recurring { get; set; }
				
        public bool IsVisible {
            get
            {
                return ScheduleType == ScheduleTypeEnum.Visible || ScheduleType == ScheduleTypeEnum.OneTimeEvent && StartRightNow;
            }
        }		
	}
}
