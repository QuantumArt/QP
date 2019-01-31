using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.TestTools.AutoFixture.Helpers;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Constants;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests.Onetime
{
    public class OnetimeTaskSchedulerTests
    {
        private readonly IFixture _fixture;

        public OnetimeTaskSchedulerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization(){ ConfigureMembers = true});
            FixtureArticleHelpers.InjectSimpleArticle(_fixture);
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenBeforeRangeTimeAndRangeHasEnded_ShouldNotDoAnything()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = startDate.AddYears(1);
            var dbDate = startDate.AddTicks(-1);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenInRangeTimeLowerBoundaryAndRangeHasEnded_ShouldShowArticle()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = startDate.AddYears(1);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(startDate);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenInRangeTimeAndRangeHasEnded_ShouldShowArticle()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = startDate.AddYears(1);
            var dbDate = new DateTime((startDate.Ticks + endDate.Ticks) / 2);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenInRangeTimeUpperBoundaryAndRangeHasEnded_ShouldShowArticle()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = startDate.AddYears(1);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(endDate);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenAfterRangeTimeAndRangeHasEnded_ShouldHideArticleAndCloseSchedule()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = startDate.AddYears(1);
            var dbDate = endDate.AddTicks(1);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(dbDate);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(task.Id), Times.Once());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenInRangeTimeAndRangeHasNotEnded_ShouldShowArticle()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = FixtureDateTimeHelpers.GenerateDateInFuture(_fixture);
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(DateTime.Now);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "OnetimeTaskRunner")]
        public void GivenOnetimeTask_WhenInRangeTimeAndRangeIsInfinitive_ShouldShowArticleAndCloseSchedule()
        {
            // Fixture setup
            var startDate = FixtureDateTimeHelpers.GenerateDateInPast(_fixture);
            var endDate = ArticleScheduleConstants.Infinity;
            var task = FixtureArticleScheduleTaskHelpers.GenerateArticleScheduleTask(_fixture, startDate, endDate);

            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(DateTime.Now);

            var sut = _fixture.Create<OnetimeTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            onetimeService.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(f => f.ShowAndCloseSchedule(task.Id), Times.Once());
            onetimeService.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }
    }
}
