using System;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler.Publishing
{
    /// <summary>
    /// Выполняет публикацию статьи по расписанию
    /// </summary>
    internal class PublishingTaskScheduler
    {
        private readonly IArticlePublishingSchedulerService _bllService;
        private readonly IOperationsLogWriter _operationsLogWriter;

        public PublishingTaskScheduler(IArticlePublishingSchedulerService bllService, IOperationsLogWriter operationsLogWriter)
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

        public void Run(PublishingTask task)
        {
            var currentTime = _bllService.GetCurrentDBDateTime();
            if (currentTime >= task.PublishingDateTime)
            {
                var article = _bllService.PublishAndCloseSchedule(task.Id);
                _operationsLogWriter.PublishArticle(article);
            }
        }
    }
}
