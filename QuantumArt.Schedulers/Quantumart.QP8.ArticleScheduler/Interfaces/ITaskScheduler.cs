using System;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    internal interface ITaskScheduler
    {
        void Run(ArticleScheduleTask articleTask);

        bool ShouldProcessTask(ISchedulerTask task, DateTime dateTimeToCheck);

        bool ShouldProcessTask(ArticleScheduleTask task, DateTime dateTimeToCheck);
    }
}
