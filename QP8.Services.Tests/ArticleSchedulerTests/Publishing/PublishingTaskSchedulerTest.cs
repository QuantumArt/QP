//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Quantumart.QP8.ArticleScheduler;
//using Quantumart.QP8.ArticleScheduler.Publishing;
//using Quantumart.QP8.BLL.Services.ArticleScheduler;

//namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler.Publishing
//{
//    [TestClass]
//    public class PublishingTaskSchedulerTest
//    {
//        private readonly IOperationsLogWriter _operationsLogWriter;

//        public PublishingTaskSchedulerTest()
//        {
//            _operationsLogWriter = new Mock<IOperationsLogWriter>().Object;
//        }

//        [TestMethod]
//        public void RunTest_BeforePublishTime_DoNothing()
//        {
//            var task = new PublishingTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17));
//            var vssMock = new Mock<IArticlePublishingSchedulerService>();

//            // Текущее время: 1.1.2011 10:15:17
//            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 1, 10, 15, 17));
//            new PublishingTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);
//            vssMock.Verify(f => f.PublishAndCloseSchedule(It.IsAny<int>()), Times.Never());
//        }

//        [TestMethod]
//        public void RunTest_AfterPublishTime_PublishAndCloseSchedule()
//        {
//            var task = new PublishingTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17));
//            var vssMock = new Mock<IArticlePublishingSchedulerService>();

//            // Текущее время: 1.1.2011 10:15:17
//            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 1, 12, 15, 17));
//            new PublishingTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);
//            vssMock.Verify(f => f.PublishAndCloseSchedule(task.Id), Times.Once());
//        }
//    }
//}
