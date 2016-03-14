using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Helpers;

namespace TestProject
{
    [TestClass]
    public class ScheduleHelperTest
    {
        [TestMethod]
        public void GetSqlValuesFromScheduleDateTest()
        {
            DateTime dt = new DateTime(2010, 3, 9, 5, 6, 7);
            Tuple<int, int> values = ScheduleHelper.GetSqlValuesFromScheduleDateTime(dt);
            Assert.AreEqual(20100309, values.Item1);
            Assert.AreEqual(50607, values.Item2);
        }
        
        [TestMethod]
        public void GetSqlValuesFromScheduleDateTest2()
        {
            DateTime dt = new DateTime(2010, 3, 9, 15, 6, 7);
            Tuple<int, int> values = ScheduleHelper.GetSqlValuesFromScheduleDateTime(dt);
            Assert.AreEqual(20100309, values.Item1);
            Assert.AreEqual(150607, values.Item2);
        }

		[TestMethod]
		public void GetScheduleDateFromSqlValuesTest()
		{
			int sqlDate = 20100309;			
			DateTime dt = ScheduleHelper.GetScheduleDateFromSqlValues(sqlDate);
			Assert.AreEqual(new DateTime(2010, 3, 9), dt);			
		}

		[TestMethod]
		public void GetScheduleTimeFromSqlValuesTest()
		{			
			int sqlTime = 50607;
			TimeSpan t = ScheduleHelper.GetScheduleTimeFromSqlValues(sqlTime);

			Assert.AreEqual(new TimeSpan(5, 6, 7), t);			
		}

        [TestMethod]
        public void GetScheduleDateTimeFromSqlValuesTest()
        {
            int sqlDate = 20100309;
            int sqlTime = 50607;
            DateTime dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(2010, dt.Year);
            Assert.AreEqual(3, dt.Month);
            Assert.AreEqual(9, dt.Day);
            Assert.AreEqual(5, dt.Hour);
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(7, dt.Second);
        }

        [TestMethod]
        public void GetScheduleDateTimeFromSqlValuesTest2()
        {
            int sqlDate = 20100309;
            int sqlTime = 150607;
            DateTime dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(15, dt.Hour);
        }
        [TestMethod]
        public void GetScheduleDateTimeFromSqlValuesTest3()
        {
            int sqlDate = 20100309;
            int sqlTime = 0;
            DateTime dt = ScheduleHelper.GetScheduleDateTimeFromSqlValues(sqlDate, sqlTime);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(0, dt.Minute);
            Assert.AreEqual(0, dt.Second);
        }

		[TestMethod]
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
