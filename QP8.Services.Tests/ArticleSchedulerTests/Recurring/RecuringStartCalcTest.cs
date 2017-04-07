//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Quantumart.QP8.ArticleScheduler.Recurring;

//namespace QP8.WebMvc.NUnit.Tests.Quantumart.QP8.ArticleScheduler.Recurring
//{
//    [TestClass]
//    public class RecuringStartCalcTest
//    {
//        [TestMethod]
//        public void MonthlyRelativeStartCalcTest()
//        {
//            IRecuringStartCalc startCalculator = new MonthlyRelativeStartCalc(9, 4, 5, new DateTime(2011, 1, 5), new DateTime(2011, 12, 31), new TimeSpan(12, 15, 17));

//            // 30.12.2010 10:11:12 -> 5.1.2011 12:15:17
//            var r = startCalculator.GetStart(new DateTime(2010, 12, 30, 10, 11, 12));
//            Assert.IsNull(r);

//            // 5.1.2011 10:10:11 -> 5.1.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 1, 5, 10, 10, 11));
//            Assert.IsNull(r);

//            // 5.1.2011 12:15:17 -> 5.1.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 1, 5, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 1, 5, 12, 15, 17), r.Value);

//            // 5.1.2011 13:15:17 -> 5.1.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 1, 5, 13, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 1, 5, 12, 15, 17), r.Value);

//            r = startCalculator.GetStart(new DateTime(2011, 4, 15));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 1, 5, 12, 15, 17), r.Value);


//            // 3.6.2011 10:15:17 -> 5.1.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 10, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 1, 5, 12, 15, 17), r.Value);

//            // 3.6.2011 12:15:17 -> 3.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 3, 12, 15, 17), r.Value);

//            // 3.6.2011 13:15:17 -> 3.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 13, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 3, 12, 15, 17), r.Value);


//            r = startCalculator.GetStart(new DateTime(2011, 9, 11));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 3, 12, 15, 17), r.Value);

//            r = startCalculator.GetStart(new DateTime(2011, 12, 5));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 11, 3, 12, 15, 17), r.Value);

//            r = startCalculator.GetStart(new DateTime(2011, 12, 31, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 11, 3, 12, 15, 17), r.Value);

//            r = startCalculator.GetStart(new DateTime(2011, 12, 31, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 11, 3, 12, 15, 17), r.Value);

//            r = startCalculator.GetStart(new DateTime(2012, 3, 10));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 11, 3, 12, 15, 17), r.Value);
//        }

//        [TestMethod]
//        public void MonthlyStartCalcTest()
//        {
//            // Каждый 15-й день каждого 3 месяца с 16.03.2011 по 24.11.2011 (старт в 12:15:17)
//            IRecuringStartCalc startCalculator = new MonthlyStartCalc(15, 3, new DateTime(2011, 3, 16), new DateTime(2011, 11, 24), new TimeSpan(12, 15, 17));

//            // 15.03.2011 12:15:17 -> 15.6.2011 12:15:17
//            var r = startCalculator.GetStart(new DateTime(2011, 3, 15, 12, 15, 17));
//            Assert.IsNull(r);

//            // 10.04.2011 12:15:17 -> 15.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 4, 10, 12, 15, 17));
//            Assert.IsNull(r);

//            // 15.6.2011 12:15:17 -> 15.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 15, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 15, 12, 15, 17), r.Value);

//            // 10.08.2011 12:15:17 -> 15.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 8, 10, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 15, 12, 15, 17), r.Value);

//            // 15.09.2011 10:15:17 -> 15.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 9, 15, 10, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 15, 12, 15, 17), r.Value);

//            // 15.09.2011 12:15:17 -> 15.9.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 9, 15, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 9, 15, 12, 15, 17), r.Value);

//            // 15.09.2011 12:15:18 -> 15.9.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 9, 15, 12, 15, 18));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 9, 15, 12, 15, 17), r.Value);

//            // 10.10.2011 12:15:17 -> 15.9.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 10, 10, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 9, 15, 12, 15, 17), r.Value);

//            // 24.11.2011 12:15:17 -> 15.9.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 11, 24, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 9, 15, 12, 15, 17), r.Value);

//            // 15.1.2012 12:15:17 -> 15.9.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2012, 1, 15, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 9, 15, 12, 15, 17), r.Value);
//        }

//        [TestMethod]
//        public void WeeklyStartCalcTest()
//        {
//            // Пн. Ср. Пн. каждые 3 недели с 1.6.2011 по 12.7.2011 (старт в 12:15:17)
//            IRecuringStartCalc startCalculator = new WeeklyStartCalc(42, 3, new DateTime(2011, 6, 1), new DateTime(2011, 7, 12), new TimeSpan(12, 15, 17));

//            // 20.05.2011 12:15:17 -> 1.6.2011 12:15:17
//            var r = startCalculator.GetStart(new DateTime(2011, 5, 20, 12, 15, 17));
//            Assert.IsNull(r);

//            // 1.06.2011 10:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 1, 10, 15, 17));
//            Assert.IsNull(r);

//            // 1.06.2011 12:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 1, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 1, 12, 15, 17), r.Value);

//            // 1.06.2011 14:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 1, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 1, 12, 15, 17), r.Value);

//            // 2.06.2011 14:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 2, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 1, 12, 15, 17), r.Value);

//            // 3.06.2011 10:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 10, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 1, 12, 15, 17), r.Value);

//            // 3.06.2011 12:15:17 -> 3.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 3, 12, 15, 17), r.Value);

//            // 3.06.2011 14:15:17 -> 3.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 3, 12, 15, 17), r.Value);

//            // 23.06.2011 14:15:17 -> 22.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 23, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 22, 12, 15, 17), r.Value);

//            // 10.07.2011 14:15:17 -> 24.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 7, 10, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 24, 12, 15, 17), r.Value);

//            // 11.07.2011 10:15:17 -> 24.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 7, 11, 10, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 24, 12, 15, 17), r.Value);

//            // 11.07.2011 12:15:17 -> 11.07.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 7, 11, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 7, 11, 12, 15, 17), r.Value);

//            // 11.07.2011 14:15:17 -> 11.07.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 7, 11, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 7, 11, 12, 15, 17), r.Value);

//            // 20.07.2011 14:15:17 -> 11.07.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 7, 20, 14, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 7, 11, 12, 15, 17), r.Value);
//        }

//        [TestMethod]
//        public void DailyStartCalcTest()
//        {
//            // Каждый 4 день с 1.6.2011 по 10.6.2011 (старт в 12:15:17)
//            IRecuringStartCalc startCalculator = new DailyStartCalc(4, new DateTime(2011, 6, 1), new DateTime(2011, 6, 10), new TimeSpan(12, 15, 17));

//            // 30.05.2011 12:15:17 -> 1.6.2011 12:15:17
//            var r = startCalculator.GetStart(new DateTime(2011, 5, 30, 12, 15, 17));
//            Assert.IsNull(r);

//            // 3.06.2011 12:15:17 -> 1.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 3, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 1, 12, 15, 17), r.Value);

//            // 7.06.2011 12:15:17 -> 5.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 7, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 5, 12, 15, 17), r.Value);

//            // 10.06.2011 12:15:17 -> 9.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 10, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 9, 12, 15, 17), r.Value);

//            // 12.06.2011 12:15:17 -> 9.6.2011 12:15:17
//            r = startCalculator.GetStart(new DateTime(2011, 6, 12, 12, 15, 17));
//            Assert.IsNotNull(r);
//            Assert.AreEqual(new DateTime(2011, 6, 9, 12, 15, 17), r.Value);
//        }
//    }
//}
