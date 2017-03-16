using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.ArticleScheduler;

namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler
{
    [TestClass]
    public class DateRangeExtensionsTest
    {
        [TestMethod]
        public void EveryMonthsTest_TwoMonthInResultRange()
        {
            var result = Tuple.Create(new DateTime(2011, 5, 7, 5, 4, 2), new DateTime(2011, 12, 27, 15, 14, 12)).EveryMonths(4).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTime(2011, 5, 1), result[0].Item1);
            Assert.AreEqual(new DateTime(2011, 5, 31), result[0].Item2);
            Assert.AreEqual(new DateTime(2011, 9, 1), result[1].Item1);
            Assert.AreEqual(new DateTime(2011, 9, 30), result[1].Item2);
        }

        [TestMethod]
        public void EveryMonthsTest_OneMonthInInputRange_OneMonthInResultRange()
        {
            var result = Tuple.Create(new DateTime(2011, 5, 7, 5, 4, 2), new DateTime(2011, 5, 27, 15, 14, 12)).EveryMonths(4).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new DateTime(2011, 5, 1), result[0].Item1);
            Assert.AreEqual(new DateTime(2011, 5, 31), result[0].Item2);
        }

        [TestMethod]
        public void EveryWeeksTest_OneWeekInInputRange_OneWeekInResultRange()
        {
            var result = Tuple.Create(new DateTime(2011, 6, 22, 5, 4, 2), new DateTime(2011, 6, 23, 5, 4, 2)).EveryWeeks(4).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new DateTime(2011, 6, 20), result[0].Item1);
            Assert.AreEqual(new DateTime(2011, 6, 26), result[0].Item2);
        }

        [TestMethod]
        public void EveryWeeksTest_TwoWeekInResultRange()
        {
            var result = Tuple.Create(new DateTime(2011, 6, 1, 5, 4, 2), new DateTime(2011, 7, 1, 5, 4, 2)).EveryWeeks(3).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTime(2011, 5, 30), result[0].Item1);
            Assert.AreEqual(new DateTime(2011, 6, 5), result[0].Item2);
            Assert.AreEqual(new DateTime(2011, 6, 20), result[1].Item1);
            Assert.AreEqual(new DateTime(2011, 6, 26), result[1].Item2);
        }

        [TestMethod]
        public void EveryDaysTest_TwoWeekInResultRange()
        {
            var result = Tuple.Create(new DateTime(2011, 6, 1, 5, 4, 2), new DateTime(2011, 6, 10, 5, 4, 2)).EveryDays(5).ToList();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTime(2011, 6, 1), result[0]);
            Assert.AreEqual(new DateTime(2011, 6, 6), result[1]);
        }

        [TestMethod]
        public void DaysTest()
        {
            var result = new[]
            {
                new Tuple<DateTime, DateTime>(new DateTime(2011, 5, 1), new DateTime(2011, 5, 31)),
                new Tuple<DateTime, DateTime>(new DateTime(2011, 9, 1), new DateTime(2011, 9, 30)),
            }.Days().Distinct().ToList();


            Assert.IsNotNull(result);
            Assert.AreEqual(61, result.Count);
        }

        [TestMethod]
        public void DayByMonthTest()
        {
            var res = new[]
            {
                new DateTime(2011, 5, 1),
                 new DateTime(2011, 5, 15),
                new DateTime(2011, 5, 10),
                new DateTime(2011, 7, 9),
                 new DateTime(2011, 7, 11),
                new DateTime(2011, 7, 1),
            }.DayByMonth(2).ToList();

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(res[0], new DateTime(2011, 5, 10));
            Assert.AreEqual(res[1], new DateTime(2011, 7, 9));
        }

        [TestMethod]
        public void LastByMonthTest()
        {
            var res = new[]
            {
                new DateTime(2011, 5, 1),
                 new DateTime(2011, 5, 15),
                new DateTime(2011, 5, 10),
                new DateTime(2011, 7, 9),
                 new DateTime(2011, 7, 11),
                new DateTime(2011, 7, 1),
            }.LastByMonth().ToList();

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(new DateTime(2011, 5, 15), res[0]);
            Assert.AreEqual(new DateTime(2011, 7, 11), res[1]);
        }

        [TestMethod]
        public void NearestTest_ReturnPreviousDate()
        {
            var dt = new DateTime(2011, 5, 14);
            var res = new[]
            {
                new DateTime(2011, 5, 1),
                 new DateTime(2011, 5, 15),
                new DateTime(2011, 5, 10)
            }.Nearest(dt);

            Assert.AreEqual(new DateTime(2011, 5, 10), res);
        }

        [TestMethod]
        public void NearestTest_ReturnNullDate()
        {
            var dt = new DateTime(2011, 4, 30);
            var res = new[]
            {
                new DateTime(2011, 5, 1),
                 new DateTime(2011, 5, 15),
                new DateTime(2011, 5, 10),
            }.Nearest(dt);

            Assert.IsNull(res);
        }

        [TestMethod]
        public void NearestTest_ReturnSameDate()
        {
            var dt = new DateTime(2011, 5, 15);
            var res = new[]
            {
                new DateTime(2011, 5, 1),
                 new DateTime(2011, 5, 15),
                new DateTime(2011, 5, 10),
            }.Nearest(dt);

            Assert.AreEqual(new DateTime(2011, 5, 15), res);
        }

        [TestMethod]
        public void NearestTest_EmptyInput_ReturnNull()
        {
            var dt = new DateTime(2011, 5, 15);
            var res = new DateTime[0].Nearest(dt);

            Assert.IsNull(res);
        }

        [TestMethod]
        public void GetWeekTest()
        {
            var res = new DateTime(2011, 6, 20).GetWeek();
            Assert.AreEqual(new DateTime(2011, 6, 20), res);

            res = new DateTime(2011, 6, 24).GetWeek();
            Assert.AreEqual(new DateTime(2011, 6, 20), res);
        }
    }
}
