using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Constants;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests.Recurring
{
    public class RecurringTaskSchedulerTests
    {
        private readonly IFixture _fixture;

        public RecurringTaskSchedulerTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
            FixtureArticleHelpers.InjectSimpleArticle(_fixture);
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Theory, AutoData, Trait("ArticleScheduler", "RecurringTaskRunner")]
        public void GivenRecurringTask_WhenDateBeforeRange_ShouldNotDoAnything(TimeSpan diffTime)
        {
            // Fixture setup
            var task = _fixture
                .Build<ArticleScheduleTask>().With(article => article.FreqType, ScheduleFreqTypes.RecurringMonthlyRelative)
                .Create();

            var startDateTime = task.StartDate + task.StartTime;
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(startDateTime - diffTime);

            var sut = _fixture.Create<RecurringTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            recurringService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "RecurringTaskRunner")]
        public void GivenRecurringTask_WhenDateWithinRangeButBeforeStartTime_ShouldNotDoAnything()
        {
            // Fixture setup
            var task = _fixture
                .Build<ArticleScheduleTask>().With(article => article.FreqType, ScheduleFreqTypes.RecurringMonthlyRelative)
                .Create();

            var startDateTime = task.StartDate + task.StartTime;
            var dbDate = startDateTime.AddTicks(-1);
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<RecurringTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            recurringService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Theory, Trait("ArticleScheduler", "RecurringTaskRunner")]
        [InlineData("01/05/2011 12:15:17")]
        [InlineData("01/10/2011 12:15:17")]
        public void GivenRecurringTask_WhenDateWithinVisibleRange_ShouldShowArticle(string rawCurrentDate)
        {
            // Fixture setup
            var startDate = DateTimeHelpers.ParseDateTime("01/01/2011");
            var endDate = DateTimeHelpers.ParseDateTime("12/31/2011");
            var startTime = DateTimeHelpers.ParseTime("12:15:17");
            var task = new ArticleScheduleTask
            {
                Id = _fixture.Create<int>(),
                ArticleId = _fixture.Create<int>(),
                FreqType = ScheduleFreqTypes.RecurringMonthlyRelative,
                FreqInterval = 9,
                FreqRelativeInterval = 4,
                FreqRecurrenceFactor = 5,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                Duration = TimeSpan.FromDays(5)
            };

            var dbDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<RecurringTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            recurringService.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            recurringService.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [InlineData("01/10/2011 13:15:17")]
        [Theory, Trait("ArticleScheduler", "RecurringTaskRunner")]
        public void GivenRecurringTask_WhenDateOutOfVisibleRange_ShouldHideArticle(string rawCurrentDate)
        {
            // Fixture setup
            var startDate = DateTimeHelpers.ParseDateTime("01/01/2011");
            var endDate = DateTimeHelpers.ParseDateTime("12/31/2011");
            var startTime = DateTimeHelpers.ParseTime("12:15:17");
            var task = new ArticleScheduleTask
            {
                Id = _fixture.Create<int>(),
                ArticleId = _fixture.Create<int>(),
                FreqType = ScheduleFreqTypes.RecurringMonthlyRelative,
                FreqInterval = 9,
                FreqRelativeInterval = 4,
                FreqRecurrenceFactor = 5,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                Duration = TimeSpan.FromDays(5)
            };

            var dbDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<RecurringTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            recurringService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideArticle(task.ArticleId), Times.Once());
            recurringService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [InlineData("01/01/2012 13:15:17")]
        [Theory, Trait("ArticleScheduler", "RecurringTaskRunner")]
        public void GivenRecurringTask_WhenDateAfterRange_ShouldHideArticleAndCloseSchedule(string rawCurrentDate)
        {
            // Fixture setup
            var startDate = DateTimeHelpers.ParseDateTime("01/01/2011");
            var endDate = DateTimeHelpers.ParseDateTime("12/31/2011");
            var startTime = DateTimeHelpers.ParseTime("12:15:17");
            var task = new ArticleScheduleTask
            {
                Id = _fixture.Create<int>(),
                ArticleId = _fixture.Create<int>(),
                FreqType = ScheduleFreqTypes.RecurringMonthlyRelative,
                FreqInterval = 9,
                FreqRelativeInterval = 4,
                FreqRecurrenceFactor = 5,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                Duration = TimeSpan.FromDays(5)
            };

            var dbDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<RecurringTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            recurringService.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(f => f.HideAndCloseSchedule(task.Id), Times.Once());
        }
    }
}
