//using System;
//using Microsoft.Practices.Unity;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Quantumart.QP8.ArticleScheduler;
//using Quantumart.QP8.BLL;
//using Quantumart.QP8.BLL.Services.ArticleScheduler;

//namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler
//{
//    [TestClass]
//    public class DbSchedulerTest
//    {
//        [TestMethod]
//        public void ParallelRunTest()
//        {
//            var articleSchedulerServiceMock = new Mock<IArticleSchedulerService>();
//            articleSchedulerServiceMock.Setup(f => f.GetScheduleTaskList()).Returns(new[]
//            {
//                new ArticleScheduleTask{Id = 1, ArticleId = 2,
//                    FreqType = 32,
//                    FreqInterval = 9,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12,15,17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18,15,17),
//                    Duration = TimeSpan.FromDays(5)
//                },

//                new ArticleScheduleTask{Id = 1, ArticleId = 2,
//                    FreqType = 4,
//                    FreqInterval = 1,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12,15,17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18,15,17),
//                    Duration = TimeSpan.FromDays(5)
//                },

//                new ArticleScheduleTask{Id = 1, ArticleId = 2,
//                    FreqType = 2,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12,15,17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18,15,17),
//                    Duration = TimeSpan.FromDays(5)
//                },

//                new ArticleScheduleTask{Id = 1, ArticleId = 2,
//                    FreqType = 1,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12,15,17),
//                    EndDate = new DateTime(2011, 12, 31),
//                    EndTime = new TimeSpan(18,15,17),
//                    Duration = TimeSpan.FromDays(5)
//                },

//                new ArticleScheduleTask{Id = 1, ArticleId = 2,
//                    FreqType = 1,
//                    FreqInterval = 3,
//                    FreqRelativeInterval = 4,
//                    FreqRecurrenceFactor = 5,
//                    StartDate = new DateTime(2011, 1, 1),
//                    StartTime = new TimeSpan(12,15,17),
//                    EndDate = new DateTime(2011, 1, 31),
//                    EndTime = new TimeSpan(18,15,17),
//                    Duration = TimeSpan.FromDays(5)
//                }
//            });

//            var articleRecurringSchedulerService = new Mock<IArticleRecurringSchedulerService>();
//            articleRecurringSchedulerService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));

//            var articlePublishingSchedulerService = new Mock<IArticlePublishingSchedulerService>();
//            articlePublishingSchedulerService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));

//            var articleOnetimeSchedulerService = new Mock<IArticleOnetimeSchedulerService>();
//            articleOnetimeSchedulerService.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));

//            var operationsLogWriter = new Mock<IOperationsLogWriter>().Object;
//            var uc = new UnityContainer()
//                .RegisterInstance(articleSchedulerServiceMock.Object)
//                .RegisterInstance(articleRecurringSchedulerService.Object)
//                .RegisterInstance(articlePublishingSchedulerService.Object)
//                .RegisterInstance(articleOnetimeSchedulerService.Object)
//                .RegisterInstance(operationsLogWriter);

//            // Вызываем метод
//            new DbScheduler("connection String", uc).Run();

//            // Проверяем что все что нужно сделано
//            articleRecurringSchedulerService.Verify(f => f.ShowArticle(2), Times.Exactly(2));
//            articlePublishingSchedulerService.Verify(f => f.PublishAndCloseSchedule(1), Times.Once());
//            articleOnetimeSchedulerService.Verify(f => f.ShowArticle(2), Times.Exactly(2));
//        }
//    }
//}
