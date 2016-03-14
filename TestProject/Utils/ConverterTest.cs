using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;
using System.Globalization;
using System.Threading;

namespace WebMvc.Test.Utils
{
    [TestClass]
    public class ConverterTest
    {
        [TestMethod]
        public void TryConvertToSqlDateString_NotNullTimeAndCorrectFormat_CorrectResult()
        {
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			string sqlDateString;
            DateTime? dateTime;
			bool result = Converter.TryConvertToSqlDateString("5/17/2011", new TimeSpan(23, 59, 59), out sqlDateString, out dateTime);

            Assert.IsTrue(result);
            Assert.IsNotNull(dateTime);
            Assert.AreEqual(new DateTime(2011, 5, 17, 23, 59, 59), dateTime);
            Assert.AreEqual("20110517 23:59:59", sqlDateString);
        }

        [TestMethod]
        public void TryConvertToSqlDateString_NullTimeAndCorrectFormat_CorrectResult()
        {
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string sqlDateString;
            DateTime? dateTime;

			bool result = Converter.TryConvertToSqlDateString("5/17/2011", null, out sqlDateString, out dateTime);

            Assert.IsTrue(result);
            Assert.IsNotNull(dateTime);
            Assert.AreEqual(new DateTime(2011, 5, 17, 0, 0, 0), dateTime);
            Assert.AreEqual("20110517 00:00:00", sqlDateString);
        }

        [TestMethod]
        public void TryConvertToSqlDateString_IncorrectFormat_EmptyResult()
        {
            string sqlDateString;
            DateTime? dateTime;

			bool result = Converter.TryConvertToSqlDateString("2011/17/5", null, out sqlDateString, out dateTime);

            Assert.IsFalse(result);
            Assert.IsNull(dateTime);
            Assert.AreEqual(String.Empty, sqlDateString);
        }

        [TestMethod]
        public void TryConvertToSqlTimeString_CorrectFormat_CorrectResult()
        {			
			string sqlTimeString;
            TimeSpan? timeSpan;
			bool result = Converter.TryConvertToSqlTimeString("10:58:41 PM", out sqlTimeString, out timeSpan);

            Assert.IsTrue(result);
            Assert.IsNotNull(timeSpan);
            Assert.AreEqual(new TimeSpan(22, 58, 41), timeSpan);
            Assert.AreEqual("22:58:41", sqlTimeString);
        }

        [TestMethod]
        public void TryConvertToSqlTimeString_IncorrectFormat_EmptyResult()
        {
            string sqlTimeString;
            TimeSpan? timeSpan;
			bool result = Converter.TryConvertToSqlTimeString("22:58 PM", out sqlTimeString, out timeSpan, "h:mm:ss tt");

            Assert.IsFalse(result);
            Assert.IsNull(timeSpan);
            Assert.AreEqual(String.Empty, sqlTimeString);
        }
    }
}
