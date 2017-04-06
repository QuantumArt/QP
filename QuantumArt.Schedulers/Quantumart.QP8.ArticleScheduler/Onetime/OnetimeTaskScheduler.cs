using System;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler.Onetime
{
    internal class OnetimeTaskScheduler
    {
        private readonly IArticleOnetimeSchedulerService _bllService;
        private readonly IOperationsLogWriter _operationsLogWriter;

        public OnetimeTaskScheduler(IArticleOnetimeSchedulerService bllService, IOperationsLogWriter operationsLogWriter)
        {
            if (bllService == null)
            {
                throw new ArgumentNullException(nameof(bllService));
            }

            if (operationsLogWriter == null)
            {
                throw new ArgumentNullException(nameof(operationsLogWriter));
            }

            _bllService = bllService;
            _operationsLogWriter = operationsLogWriter;
        }

        public void Run(OnetimeTask task)
        {
            var range = Tuple.Create(task.StartDateTime, task.EndDateTime);
            var currentTime = _bllService.GetCurrentDBDateTime();
            var pos = range.Position(currentTime);

            Article article;
            if (pos > 0)
            {
                // после диапазона задачи
                article = _bllService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    _operationsLogWriter.HideArticle(article);
                }
            }
            else if (pos == 0)
            {
                // в диапазоне
                article = task.EndDateTime.Year == ArticleScheduleConstants.Infinity.Year ? _bllService.ShowAndCloseSchedule(task.Id) : _bllService.ShowArticle(task.ArticleId);
                if (article != null && !article.Visible)
                {
                    _operationsLogWriter.ShowArticle(article);
                }
            }
        }
    }
}
