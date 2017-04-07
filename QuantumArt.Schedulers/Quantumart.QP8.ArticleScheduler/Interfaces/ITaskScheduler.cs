using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler.Interfaces
{
    internal interface ITaskScheduler
    {
        void Run(ArticleScheduleTask articleTask);
    }
}
