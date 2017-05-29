using System;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using QP8.Infrastructure.Logging.Factories;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests.Publishing
{
    public class PublishingTaskSchedulerTest
    {
        private readonly IFixture _fixture;

        public PublishingTaskSchedulerTest()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
            FixtureArticleHelpers.InjectSimpleArticle(_fixture);
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Theory, AutoData, Trait("ArticleScheduler", "PublishingTaskRunner")]
        public void GivenPublishingTask_WhenBeforePublishTime_ShouldNotDoAnything(ArticleScheduleTask task, TimeSpan diffTime)
        {
            // Fixture setup
            var startDateTime = task.StartDate + task.StartTime;
            var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();
            publishingService.Setup(m => m.GetCurrentDBDateTime()).Returns(startDateTime - diffTime);

            var sut = _fixture.Create<PublishingTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            publishingService.Verify(f => f.PublishAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Theory, AutoData, Trait("ArticleScheduler", "PublishingTaskRunner")]
        public void GivenPublishingTask_WhenPublishTimeTheSame_ShouldPublishArticleAndCloseSchedule(ArticleScheduleTask task)
        {
            // Fixture setup
            var startDateTime = task.StartDate + task.StartTime;
            var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();
            publishingService.Setup(m => m.GetCurrentDBDateTime()).Returns(startDateTime);

            var sut = _fixture.Create<PublishingTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            publishingService.Verify(f => f.PublishAndCloseSchedule(It.IsAny<int>()), Times.Once());
        }

        [Theory, AutoData, Trait("ArticleScheduler", "PublishingTaskRunner")]
        public void GivenPublishingTask_WhenAfterPublishTime_ShouldPublishArticleAndCloseSchedule(ArticleScheduleTask task, TimeSpan diffTime)
        {
            // Fixture setup
            var startDateTime = task.StartDate + task.StartTime;
            var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();
            publishingService.Setup(m => m.GetCurrentDBDateTime()).Returns(startDateTime + diffTime);

            var sut = _fixture.Create<PublishingTaskScheduler>();

            // Exercise system
            sut.Run(task);

            // Verify outcome
            publishingService.Verify(f => f.PublishAndCloseSchedule(It.IsAny<int>()), Times.Once());
        }
    }
}
