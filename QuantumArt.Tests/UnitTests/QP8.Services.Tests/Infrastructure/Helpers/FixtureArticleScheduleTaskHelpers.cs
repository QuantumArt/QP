using System;
using AutoFixture;
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
                .Create();
        }
    }
}
