using System;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using Quantumart.QP8.Constants;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests
{
    public class DbSchedulerTests
    {
        private readonly IFixture _fixture;

        public DbSchedulerTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
            FixtureArticleHelpers.InjectSimpleArticle(_fixture);
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Fact, Trait("ArticleScheduler", "TaskRunners")]
        public void GivenSomeTaskList_WhenScheduleIsCorrect_ShouldCallAllTaskRunners()
        {
            // Fixture setup
            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
            var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();
            var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();

            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
            publishingService.Setup(m => m.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
            schedulerService.Setup(m => m.GetScheduleTaskList()).Returns(GetArticleSchedulerTaskList);

            _fixture.Register<IOnetimeTaskScheduler>(_fixture.Create<OnetimeTaskScheduler>);
            _fixture.Register<IRecurringTaskScheduler>(_fixture.Create<RecurringTaskScheduler>);
            _fixture.Register<IPublishingTaskScheduler>(_fixture.Create<PublishingTaskScheduler>);

            var sut = _fixture.Create<DbScheduler>();

            // Exercise system
            sut.Run();

            // Verify outcome
            recurringService.Verify(m => m.ShowArticle(2), Times.Exactly(2));
            publishingService.Verify(m => m.PublishAndCloseSchedule(1), Times.Once());
            onetimeService.Verify(m => m.ShowArticle(2), Times.Exactly(2));
        }

        private static ArticleScheduleTask[] GetArticleSchedulerTaskList()
        {
            return new[]
            {
                new ArticleScheduleTask
                {
                    Id = 1,
                    ArticleId = 2,
                    FreqType = ScheduleFreqTypes.RecurringMonthlyRelative,
                    FreqInterval = 9,
                    FreqRelativeInterval = 4,
                    FreqRecurrenceFactor = 5,
                    StartDate = new DateTime(2011, 1, 1),
                    StartTime = new TimeSpan(12, 15, 17),
                    EndDate = new DateTime(2011, 12, 31),
                    EndTime = new TimeSpan(18, 15, 17),
                    Duration = TimeSpan.FromDays(5)
                },
                new ArticleScheduleTask
                {
                    Id = 1,
                    ArticleId = 2,
                    FreqType = ScheduleFreqTypes.RecurringDaily,
                    FreqInterval = 1,
                    FreqRelativeInterval = 4,
                    FreqRecurrenceFactor = 5,
                    StartDate = new DateTime(2011, 1, 1),
                    StartTime = new TimeSpan(12, 15, 17),
                    EndDate = new DateTime(2011, 12, 31),
                    EndTime = new TimeSpan(18, 15, 17),
                    Duration = TimeSpan.FromDays(5)
                },
                new ArticleScheduleTask
                {
                    Id = 1,
                    ArticleId = 2,
                    FreqType = ScheduleFreqTypes.Publishing,
                    FreqInterval = 3,
                    FreqRelativeInterval = 4,
                    FreqRecurrenceFactor = 5,
                    StartDate = new DateTime(2011, 1, 1),
                    StartTime = new TimeSpan(12, 15, 17),
                    EndDate = new DateTime(2011, 12, 31),
                    EndTime = new TimeSpan(18, 15, 17),
                    Duration = TimeSpan.FromDays(5)
                },
                new ArticleScheduleTask
                {
                    Id = 1,
                    ArticleId = 2,
                    FreqType = ScheduleFreqTypes.OneTime,
                    FreqInterval = 3,
                    FreqRelativeInterval = 4,
                    FreqRecurrenceFactor = 5,
                    StartDate = new DateTime(2011, 1, 1),
                    StartTime = new TimeSpan(12, 15, 17),
                    EndDate = new DateTime(2011, 12, 31),
                    EndTime = new TimeSpan(18, 15, 17),
                    Duration = TimeSpan.FromDays(5)
                },
                new ArticleScheduleTask
                {
                    Id = 1,
                    ArticleId = 2,
                    FreqType = ScheduleFreqTypes.OneTime,
                    FreqInterval = 3,
                    FreqRelativeInterval = 4,
                    FreqRecurrenceFactor = 5,
                    StartDate = new DateTime(2011, 1, 1),
                    StartTime = new TimeSpan(12, 15, 17),
                    EndDate = new DateTime(2011, 1, 31),
                    EndTime = new TimeSpan(18, 15, 17),
                    Duration = TimeSpan.FromDays(5)
                }
            };
        }
    }
}
