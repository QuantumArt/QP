using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    internal class DbScheduler : IScheduler
    {
        private readonly IOnetimeTaskScheduler _onetimeScheduler;
        private readonly IRecurringTaskScheduler _recurringScheduler;
        private readonly IPublishingTaskScheduler _publishingScheduler;
        private readonly IArticleSchedulerService _articleSchedulerService;

        public DbScheduler(
            IOnetimeTaskScheduler onetimeScheduler,
            IRecurringTaskScheduler recurringScheduler,
            IPublishingTaskScheduler publishingScheduler,
            IArticleSchedulerService articleSchedulerService
        )
        {
            _onetimeScheduler = onetimeScheduler;
            _recurringScheduler = recurringScheduler;
            _publishingScheduler = publishingScheduler;
            _articleSchedulerService = articleSchedulerService;
        }

        public void Run()
        {
            GetDbScheduleTaskActions().ForEach(RunScheduleTaskAction);
        }

        private IEnumerable<ArticleScheduleTask> GetDbScheduleTaskActions()
        {
            return _articleSchedulerService.GetScheduleTaskList();
        }

        private void RunScheduleTaskAction(ArticleScheduleTask articleTask)
        {
            switch (articleTask.FreqType)
            {
                case ScheduleFreqTypes.OneTime:
                    _onetimeScheduler.Run(articleTask);
                    break;
                case ScheduleFreqTypes.Publishing:
                    _publishingScheduler.Run(articleTask);
                    break;
                case ScheduleFreqTypes.RecurringDaily:
                case ScheduleFreqTypes.RecurringWeekly:
                case ScheduleFreqTypes.RecurringMonthly:
                case ScheduleFreqTypes.RecurringMonthlyRelative:
                    _recurringScheduler.Run(articleTask);
                    break;
                default:
                    throw new ArgumentException("Undefined FreqType value: " + articleTask.FreqType);
            }
        }
    }
}
