using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quantumart.QP8.ArticleScheduler;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler.Recurring
{
    [TestClass]
    public class RecurringTaskSchedulerTest
    {
        private RecurringTask _task;
        private readonly IOperationsLogWriter _operationsLogWriter;

        public RecurringTaskSchedulerTest()
        {
            _operationsLogWriter = new Mock<IOperationsLogWriter>().Object;
        }

        [TestInitialize()]
        public void Initialize()
        {
            // С 1.1.2011 по 31.12.2011 в 12:15:17 каждый третий рабочий день каждого 5-го месяца
            // показывать статью в течении 5 дней
            _task = new RecurringTask(204, 1633, RecurringTaskTypes.MonthlyRelative, 9, 4, 5,
                new DateTime(2011, 1, 1), new DateTime(2011, 12, 31),
                new TimeSpan(12, 15, 17), TimeSpan.FromDays(5));
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeBeforeRangeStart_DoNothing()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 30.12.2010 10:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2010, 12, 30, 10, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // ничего не будет вызвано
            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeWithinRangeButBeforeFirstVisibleStartTime_DoNothing()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 5.1.2011 10:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 10, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // ничего не будет вызвано
            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeWithinVisibileRange_1_ShowArticle()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 5.1.2011 12:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 5, 12, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // показать статью
            vssMock.Verify(f => f.ShowArticle(_task.ArticleId), Times.Once());
            vssMock.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeWithinVisibileRange_2_ShowArticle()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 10.1.2011 12:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 10, 12, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // показать статью
            vssMock.Verify(f => f.ShowArticle(_task.ArticleId), Times.Once());
            vssMock.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeWithoutVisibileRange_HideArticle()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 10.1.2011 13:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2011, 1, 10, 13, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // скрыть статью
            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideArticle(_task.ArticleId), Times.Once());
            vssMock.Verify(f => f.HideAndCloseSchedule(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void RunTest_CurrentDatetimeAfterRange_CloseSchedule()
        {
            var vssMock = new Mock<IArticleRecurringSchedulerService>();

            // Текущее время: 1.1.2012 13:15:17
            vssMock.Setup(f => f.GetCurrentDBDateTime()).Returns(new DateTime(2012, 1, 1, 13, 15, 17));

            new RecurringTaskScheduler(vssMock.Object, _operationsLogWriter).Run(_task);

            // закрыть задачу
            vssMock.Verify(f => f.HideArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.ShowArticle(It.IsAny<int>()), Times.Never());
            vssMock.Verify(f => f.HideAndCloseSchedule(_task.Id), Times.Once());
        }
    }
}
