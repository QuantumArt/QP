using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    internal interface ITaskScheduler
    {
        void Run(ArticleScheduleTask articleTask);

        bool ShouldProcessTask(ISchedulerTask task, DateTimeOffset dateTimeToCheck);

        bool ShouldProcessTask(ArticleScheduleTask task, DateTimeOffset dateTimeToCheck);
    }
}
