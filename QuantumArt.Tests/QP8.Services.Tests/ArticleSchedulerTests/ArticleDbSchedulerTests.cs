using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.ArticleScheduler.Publishing;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.ArticleScheduler;
using Quantumart.QP8.Constants;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests
{
    public class ArticleDbSchedulerTests
    {
        private readonly IFixture _fixture;
        private const int ScheduleId = 1;
        private const int ArticleId = 2;
        private const string CurrentDateTime = "01/05/2011 12:15:17";

        public ArticleDbSchedulerTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
            FixtureArticleHelpers.InjectSimpleArticle(_fixture);
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Fact, Trait("ArticleScheduler", "TaskRunners")]
        public void GivenSomeTaskList_WhenScheduleIsCorrect_ShouldCallAllOnetimeServiceTaskRunners()
        {
            // Fixture setup
            _fixture.Register<IOnetimeTaskScheduler>(_fixture.Create<OnetimeTaskScheduler>);

            var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();
            var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();

            var dateTimeNow = DateTimeHelpers.ParseDateTime("01/05/2011 12:15:17");
            onetimeService.Setup(m => m.GetCurrentDBDateTime()).Returns(dateTimeNow);
            schedulerService.Setup(m => m.GetScheduleTaskList()).Returns(GetArticleSchedulerTaskList);

            var sut = _fixture.Create<DbScheduler>();

            // Exercise system
            sut.Run();

            // Verify outcome
            onetimeService.Verify(m => m.ShowArticle(ArticleId), Times.Exactly(3));
            onetimeService.Verify(m => m.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
            onetimeService.Verify(m => m.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "TaskRunners")]
        public void GivenSomeTaskList_WhenScheduleIsCorrect_ShouldCallAllRecurringServiceTaskRunners()
        {
            // Fixture setup
            _fixture.Register<IRecurringTaskScheduler>(_fixture.Create<RecurringTaskScheduler>);

            var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();
            var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();

            var dateTimeNow = DateTimeHelpers.ParseDateTime(CurrentDateTime);
            recurringService.Setup(m => m.GetCurrentDBDateTime()).Returns(dateTimeNow);
            schedulerService.Setup(m => m.GetScheduleTaskList()).Returns(GetArticleSchedulerTaskList);

            var sut = _fixture.Create<DbScheduler>();

            // Exercise system
            sut.Run();

            // Verify outcome
            recurringService.Verify(m => m.ShowArticle(ArticleId), Times.Exactly(2));
            recurringService.Verify(m => m.HideArticle(It.IsAny<int>()), Times.Never());
            recurringService.Verify(m => m.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [Fact, Trait("ArticleScheduler", "TaskRunners")]
        public void GivenSomeTaskList_WhenScheduleIsCorrect_ShouldCallAllPublishingServiceTaskRunners()
        {
            // Fixture setup
            _fixture.Register<IPublishingTaskScheduler>(_fixture.Create<PublishingTaskScheduler>);

            var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();
            var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();

            var dateTimeNow = DateTimeHelpers.ParseDateTime(CurrentDateTime);
            publishingService.Setup(m => m.GetCurrentDBDateTime()).Returns(dateTimeNow);
            schedulerService.Setup(m => m.GetScheduleTaskList()).Returns(GetArticleSchedulerTaskList);

            var sut = _fixture.Create<DbScheduler>();

            // Exercise system
            sut.Run();

            // Verify outcome
            publishingService.Verify(m => m.PublishAndCloseSchedule(ScheduleId), Times.Once());
        }

        [Fact, Trait("ArticleScheduler", "TaskRunners")]
        public void GivenSomeTaskListAndSpecificDate_WhenScheduleShouldRunAtNearestFuture_ShouldCalculateTasksCountQueue()
        {
            // Fixture setup
            _fixture.Register<IOnetimeTaskScheduler>(_fixture.Create<OnetimeTaskScheduler>);
            _fixture.Register<IPublishingTaskScheduler>(_fixture.Create<PublishingTaskScheduler>);

            var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();
            var tasksList = GetArticleSchedulerTaskList().Where(t => t.FreqType == ScheduleFreqTypes.OneTime || t.FreqType == ScheduleFreqTypes.Publishing);
            var dateTimeNow = DateTimeHelpers.ParseDateTime(CurrentDateTime);
            schedulerService.Setup(m => m.GetScheduleTaskList()).Returns(tasksList);

            const int expectedResultNow = 4;
            const int expectedResultAt3Hours = 6;
            var sut = _fixture.Create<DbScheduler>();

            // Exercise system
            var actualResultNow = sut.GetTasksCountToProcessAtSpecificDateTime(dateTimeNow);
            var actualResultAt3Hours = sut.GetTasksCountToProcessAtSpecificDateTime(dateTimeNow.AddHours(3));

            // Verify outcome
            Assert.Equal(expectedResultNow, actualResultNow);
            Assert.Equal(expectedResultAt3Hours, actualResultAt3Hours);
        }

        private IEnumerable<ArticleScheduleTask> GetArticleSchedulerTaskList()
        {
            var taskComposer = _fixture
                .Build<ArticleScheduleTask>()
                .With(t => t.Id, ScheduleId)
                .With(t => t.ArticleId, ArticleId)
                .With(t => t.FreqRelativeInterval, 4)
                .With(t => t.FreqRecurrenceFactor, 5)
                .With(t => t.StartDate, DateTimeHelpers.ParseDate("01/01/2011"))
                .With(t => t.StartTime, DateTimeHelpers.ParseTime("12:15:17"))
                .With(t => t.EndTime, DateTimeHelpers.ParseTime("18:15:17"))
                .With(t => t.Duration, TimeSpan.FromDays(5));

            var onetimeTaskComposer = taskComposer.With(t => t.FreqType, ScheduleFreqTypes.OneTime).With(t => t.FreqInterval, 3);
            var publishingTaskComposer = taskComposer.With(t => t.FreqType, ScheduleFreqTypes.Publishing).With(t => t.FreqInterval, 3);

            var onetimeTasks = new[]
            {
                onetimeTaskComposer.With(t => t.EndDate, DateTimeHelpers.ParseDate("01/31/2011")).Create(),
                onetimeTaskComposer.With(t => t.EndDate, DateTimeHelpers.ParseDate("12/31/2011")).Create(),
                onetimeTaskComposer.With(t => t.EndDate, DateTimeHelpers.ParseDate("01/05/2011")).With(t => t.EndTime, DateTimeHelpers.ParseTime("12:15:17")).Create(),
                onetimeTaskComposer
                    .With(t => t.StartDate, DateTimeHelpers.ParseDate(CurrentDateTime).AddHours(2))
                    .With(t => t.StartTime, DateTimeHelpers.ParseTime(CurrentDateTime))
                    .With(t => t.EndDate, DateTimeHelpers.ParseDate("12/31/2011"))
                    .Create()
            };

            var publishingTasks = new[]
            {
                publishingTaskComposer.With(t => t.EndDate, DateTimeHelpers.ParseDate("12/31/2011")).Create(),
                publishingTaskComposer
                    .With(t => t.StartDate, DateTimeHelpers.ParseDate(CurrentDateTime).AddHours(2))
                    .With(t => t.StartTime, DateTimeHelpers.ParseTime(CurrentDateTime))
                    .Create()
            };

            var recurringTasks = new[]
            {
                taskComposer.With(t => t.FreqType, ScheduleFreqTypes.RecurringDaily).With(t => t.FreqInterval, 1).With(t => t.EndDate, DateTimeHelpers.ParseDate("12/31/2011")).Create(),
                taskComposer.With(t => t.FreqType, ScheduleFreqTypes.RecurringMonthlyRelative).With(t => t.FreqInterval, 9).With(t => t.EndDate, DateTimeHelpers.ParseDate("12/31/2011")).Create()
            };

            return onetimeTasks.Concat(publishingTasks).Concat(recurringTasks);
        }
    }
}
