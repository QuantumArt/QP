using System;
using NUnit.Framework;
using Quantumart.QP8.BLL.Helpers;

namespace QP8.WebMvc.NUnit.Tests
{
    [TestFixture]
    public class ScheduleHelperTest
    {
        [Test]
        public void GetSqlValuesFromScheduleDateTest()
        {
            var dt = new DateTime(2010, 3, 9, 5, 6, 7);
            var values = ScheduleHelper.GetSqlValuesFromScheduleDateTime(dt);
            Assert.AreEqual(20100309, values.Item1);
            Assert.AreEqual(50607, values.Item2);
        }

        [Test]
        public void GetSqlValuesFromScheduleDateTest2()
        {
            var dt = new DateTime(2010, 3, 9, 15, 6, 7);
            var values = ScheduleHelper.GetSqlValuesFromScheduleDateTime(dt);
            Assert.AreEqual(20100309, values.Item1);
            Assert.AreEqual(150607, values.Item2);
        }

        [Test]
        public void GetScheduleDateFromSqlValuesTest()
        {
            const int sqlDate = 20100309;
            var dt = ScheduleHelper.GetScheduleDateFromSqlValues(sqlDate);
            Assert.AreEqual(new DateTime(2010, 3, 9), dt);
        }

        [Test]
        public void GetScheduleTimeFromSqlValuesTest()
        {
            const int sqlTime = 50607;
            var t = ScheduleHelper.GetScheduleTimeFromSqlValues(sqlTime);

            Assert.AreEqual(new TimeSpan(5, 6, 7), t);
        }

        [Test]
        public void GetScheduleDateTimeFromSqlValuesTest()
        {
            const int sqlDate = 20100309;
            const int sqlTime = 50607;
            var dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(2010, dt.Year);
            Assert.AreEqual(3, dt.Month);
            Assert.AreEqual(9, dt.Day);
            Assert.AreEqual(5, dt.Hour);
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(7, dt.Second);
        }

        [Test]
        public void GetScheduleDateTimeFromSqlValuesTest2()
        {
            const int sqlDate = 20100309;
            const int sqlTime = 150607;
            var dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(15, dt.Hour);
        }

        [Test]
        public void GetScheduleDateTimeFromSqlValuesTest3()
        {
            const int sqlDate = 20100309;
            const int sqlTime = 0;
            var dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(0, dt.Minute);
            Assert.AreEqual(0, dt.Second);
        }

        [Test]
        public void GetDurationTest()
        {
            var duration = ScheduleHelper.GetDuration("mi", 1, DateTime.Now);
            Assert.AreEqual(TimeSpan.FromMinutes(1), duration);

            duration = ScheduleHelper.GetDuration("hh", 1, DateTime.Now);
            Assert.AreEqual(TimeSpan.FromHours(1), duration);

            duration = ScheduleHelper.GetDuration("dd", 1, DateTime.Now);
            Assert.AreEqual(TimeSpan.FromDays(1), duration);

            duration = ScheduleHelper.GetDuration("wk", 2, DateTime.Now);
            Assert.AreEqual(TimeSpan.FromDays(14), duration);

            var startDate = new DateTime(2011, 1, 1, 12, 15, 17);

            duration = ScheduleHelper.GetDuration("mm", 2, startDate);
            Assert.AreEqual(new DateTime(2011, 3, 1), (startDate + duration).Date);

            duration = ScheduleHelper.GetDuration("yy", 2, startDate);
            Assert.AreEqual(new DateTime(2013, 1, 1), (startDate + duration).Date);
        }
    }
}
