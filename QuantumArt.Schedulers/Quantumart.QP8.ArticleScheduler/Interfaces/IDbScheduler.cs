using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    internal interface IDbScheduler
    {
        void Run();

        List<ArticleScheduleTask> GetDbScheduleTaskActions();

        int GetTasksCountToProcessAtSpecificDateTime(DateTimeOffset dateTimeToCheck);
    }
}
