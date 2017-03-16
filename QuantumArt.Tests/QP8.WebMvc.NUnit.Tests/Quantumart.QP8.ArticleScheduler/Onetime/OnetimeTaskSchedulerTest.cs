using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quantumart.QP8.ArticleScheduler;
using Quantumart.QP8.ArticleScheduler.Onetime;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler.Onetime
{
    [TestClass]
    public class OnetimeTaskSchedulerTest
    {
        private readonly IOperationsLogWriter _operationsLogWriter;

        public OnetimeTaskSchedulerTest()
        {
            _operationsLogWriter = new Mock<IOperationsLogWriter>().Object;
        }

        [TestMethod]
        public void RunTest_RangeHasEnd_CurrentDateTimeIsBeforeRange_DoNothing()
        {
            var task = new OnetimeTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17), new DateTime(2011, 12, 31, 18, 10, 40));
            var vssMock = new Mock<IArticleOnetimeSchedulerService>();

            // Текущее время: 1.1.2011 10:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 1, 10, 15, 17));
            new OnetimeTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);

            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_RangeHasEnd_CurrentDateTimeWithinRange_1_ShowArticle()
        {
            var task = new OnetimeTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17), new DateTime(2011, 12, 31, 18, 10, 40));
            var vssMock = new Mock<IArticleOnetimeSchedulerService>();

            // Текущее время: 1.1.2011 12:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 1, 12, 15, 17));
            new OnetimeTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);

            vssMock.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            vssMock.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_RangeHasEnd_CurrentDateTimeWithinRange_2_ShowArticle()
        {
            var task = new OnetimeTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17), new DateTime(2011, 12, 31, 18, 10, 40));
            var vssMock = new Mock<IArticleOnetimeSchedulerService>();

            // Текущее время: 31.12.2011 18:10:40
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 12, 31, 18, 10, 40));
            new OnetimeTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);

            vssMock.Verify(f => f.ShowArticle(task.ArticleId), Times.Once());
            vssMock.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_RangeHasEnd_CurrentDateTimeAfterRange_HideAndCloseSchedule()
        {
            var task = new OnetimeTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17), new DateTime(2011, 12, 31, 18, 10, 40));
            var vssMock = new Mock<IArticleOnetimeSchedulerService>();

            // Текущее время: 31.12.2011 18:10:41
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 12, 31, 18, 10, 41));
            new OnetimeTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);

            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.ShowAndCloseSchedule(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(task.Id), Times.Once());
        }

        [TestMethod]
        public void RunTest_RangeHasNoEnd_CurrentDateTimeWithinRange_ShowAndCloseSchedule()
        {
            var task = new OnetimeTask(204, 1633, new DateTime(2011, 1, 1, 12, 15, 17), new DateTime(9999, 12, 31, 18, 10, 40));
            var vssMock = new Mock<IArticleOnetimeSchedulerService>();

            // Текущее время: 1.1.2011 12:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 1, 12, 15, 17));
            new OnetimeTaskScheduler(vssMock.Object, _operationsLogWriter).Run(task);

            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.ShowAndCloseSchedule(task.Id), Times.Once());
        }
    }
}
