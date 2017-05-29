using System;
using Ploeh.AutoFixture;
using Quantumart.QP8.BLL;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    public static class FixtureArticleScheduleTaskHelpers
    {
        public static ArticleScheduleTask GenerateArticleScheduleTask(IFixture fixture, DateTime startDate, DateTime endDate)
        {
            return fixture
                .Build<ArticleScheduleTask>()
                .With(article => article.StartDate, startDate)
                .With(article => article.EndDate, endDate)
                .Without(article => article.StartTime)
                .Without(article => article.EndTime)
                .Create();
        }
    }
}
