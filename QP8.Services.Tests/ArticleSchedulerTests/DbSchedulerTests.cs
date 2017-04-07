//using System;
//using FluentAssertions;
//using Moq;
//using Ploeh.AutoFixture;
//using Ploeh.AutoFixture.AutoMoq;
//using QP8.Infrastructure.Logging.Factories;
//using Quantumart.QP8.BLL;
//using Quantumart.QP8.BLL.Services.ArticleScheduler;
//using Xunit;

//namespace QP8.Services.Tests.ArticleSchedulerTests
//{
//    public class DbSchedulerTests
//    {
//        private readonly IFixture _fixture;

//        public DbSchedulerTests()
//        {
//            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());

//            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
//            LogProvider.LogFactory = new NullLogFactory();
//        }

//        [Fact]
//        public void Test()
//        {
//            var xxx = _fixture.Create<IArticleRecurringSchedulerService>();
//            xxx.ShowArticle(2);
//            xxx.ShouldNotRaise(It.IsAny<string>()); ;
//        }

//        //[Fact]
//        //public void ParallelRunTest()
//        //{
//        //    // Fixture setup
//        //    var onetimeService = _fixture.Freeze<Mock<IArticleOnetimeSchedulerService>>();
//        //    var recurringService = _fixture.Freeze<Mock<IArticleRecurringSchedulerService>>();
//        //    var publishingService = _fixture.Freeze<Mock<IArticlePublishingSchedulerService>>();
//        //    var schedulerService = _fixture.Freeze<Mock<IArticleSchedulerService>>();

//        //    onetimeService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
//        //    recurringService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
//        //    publishingService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));
//        //    schedulerService.Setup(f => f.GetScheduleTaskList()).Returns(GetArticleSchedulerTaskList);

//        //    _fixture.Register<IOnetimeTaskScheduler>(_fixture.Create<OnetimeTaskScheduler>);
//        //    _fixture.Register<IRecurringTaskScheduler>(_fixture.Create<RecurringTaskScheduler>);
//        //    _fixture.Register<IPublishingTaskScheduler>(_fixture.Create<PublishingTaskScheduler>);

//        //    var sut = _fixture.Create<DbScheduler>();

//        //    // Exercise system
//        //    sut.Run();

//        //    // Verify outcome
//        //    recurringService.Verify(f => f.ShowArticle(2), Times.Exactly(2));
//        //    publishingService.Verify(f => f.PublishAndCloseSchedule(1), Times.Once());
//        //    onetimeService.Verify(f => f.ShowArticle(2), Times.Exactly(2));
//        //}

//        private static ArticleScheduleTask[] GetArticleSchedulerTaskList()
//        {
//            return new[]
//            {
//                new ArticleScheduleTask
//                {
//                    Id = 1,
//                    ArticleId = 2,
//                    FreqType = 32,
//                    FreqInterval = 9,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12, 15, 17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18, 15, 17),
//                    Duration = TimeSpan.FromDays(5)
//                },
//                new ArticleScheduleTask
//                {
//                    Id = 1,
//                    ArticleId = 2,
//                    FreqType = 4,
//                    FreqInterval = 1,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12, 15, 17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18, 15, 17),
//                    Duration = TimeSpan.FromDays(5)
//                },
//                new ArticleScheduleTask
//                {
//                    Id = 1,
//                    ArticleId = 2,
//                    FreqType = 2,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12, 15, 17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18, 15, 17),
//                    Duration = TimeSpan.FromDays(5)
//                },
//                new ArticleScheduleTask
//                {
//                    Id = 1,
//                    ArticleId = 2,
//                    FreqType = 1,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12, 15, 17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18, 15, 17),
//                    Duration = TimeSpan.FromDays(5)
//                },
//                new ArticleScheduleTask
//                {
//                    Id = 1,
//                    ArticleId = 2,
//                    FreqType = 1,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12, 15, 17),
//                    EndDate = new DateTime(2011, 1, 31),
//                    EndTime = new TimeSpan(18, 15, 17),
//                    Duration = TimeSpan.FromDays(5)
//                }
//            };
//        }
//    }
//}
