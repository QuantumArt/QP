using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
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
            dbTasks.Where(FilterOnetimeTasksPredicate).ForEach(_onetimeScheduler.Run);
            dbTasks.Where(FilterRecurringTasksPredicate).ForEach(_recurringScheduler.Run);
            dbTasks.Where(FilterPublishingTasksPredicate).ForEach(_publishingScheduler.Run);
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
