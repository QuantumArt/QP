using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    internal class DbScheduler : IDbScheduler
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
            var dbTasks = GetDbScheduleTaskActions();
            foreach (var onetimeTask in dbTasks.Where(FilterOnetimeTasksPredicate))
            {
                _onetimeScheduler.Run(onetimeTask);
            }

            foreach (var recurringTask in dbTasks.Where(FilterRecurringTasksPredicate))
            {
                _recurringScheduler.Run(recurringTask);
            }

            foreach (var publishingTask in dbTasks.Where(FilterPublishingTasksPredicate))
            {
                _publishingScheduler.Run(publishingTask);
            }
        }

        public List<ArticleScheduleTask> GetDbScheduleTaskActions() => _articleSchedulerService.GetScheduleTaskList().ToList();

        public int GetTasksCountToProcessAtSpecificDateTime(DateTime dateTimeToCheck)
        {
            var dbTasks = GetDbScheduleTaskActions();
            var onetimeTasksCount = dbTasks.Where(FilterOnetimeTasksPredicate).Count(FilterTasksToProceedPredicate(_onetimeScheduler, dateTimeToCheck));
            var recurringTasksCount = dbTasks.Where(FilterRecurringTasksPredicate).Count(FilterTasksToProceedPredicate(_recurringScheduler, dateTimeToCheck));
            var publishingTasksCount = dbTasks.Where(FilterPublishingTasksPredicate).Count(FilterTasksToProceedPredicate(_publishingScheduler, dateTimeToCheck));
            return onetimeTasksCount + recurringTasksCount + publishingTasksCount;
        }

        private static Func<ArticleScheduleTask, bool> FilterTasksToProceedPredicate(ITaskScheduler taskScheduler, DateTime dateTimeToCheck)
        {
            return task => taskScheduler.ShouldProcessTask(task, dateTimeToCheck);
        }

        private static bool FilterOnetimeTasksPredicate(ArticleScheduleTask task) => task.FreqType == ScheduleFreqTypes.OneTime;

        private static bool FilterRecurringTasksPredicate(ArticleScheduleTask task) => task.FreqType == ScheduleFreqTypes.RecurringDaily
            || task.FreqType == ScheduleFreqTypes.RecurringWeekly
            || task.FreqType == ScheduleFreqTypes.RecurringMonthly
            || task.FreqType == ScheduleFreqTypes.RecurringMonthlyRelative;

        private static bool FilterPublishingTasksPredicate(ArticleScheduleTask task) => task.FreqType == ScheduleFreqTypes.Publishing;
    }
}
