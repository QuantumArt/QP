using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.Utils
{
    [TestFixture]
    public class ConverterTest
    {
        [Test]
        public void TryConvertToSqlDateString_NotNullTimeAndCorrectFormat_CorrectResult()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var result = Converter.TryConvertToSqlDateString("5/17/2011", new TimeSpan(23, 59, 59), out var sqlDateString, out var dateTime);

            Assert.IsTrue(result);
            Assert.IsNotNull(dateTime);
            Assert.AreEqual(new DateTime(2011, 5, 17, 23, 59, 59), dateTime);
            Assert.AreEqual("20110517 23:59:59", sqlDateString);
        }

        [Test]
        public void TryConvertToSqlDateString_NullTimeAndCorrectFormat_CorrectResult()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var result = Converter.TryConvertToSqlDateString("5/17/2011", null, out var sqlDateString, out var dateTime);

            Assert.IsTrue(result);
            Assert.IsNotNull(dateTime);
            Assert.AreEqual(new DateTime(2011, 5, 17, 0, 0, 0), dateTime);
            Assert.AreEqual("20110517 00:00:00", sqlDateString);
        }

        [Test]
        public void TryConvertToSqlDateString_IncorrectFormat_EmptyResult()
        {
            var result = Converter.TryConvertToSqlDateString("2011/17/5", null, out var sqlDateString, out var dateTime);

            Assert.IsFalse(result);
            Assert.IsNull(dateTime);
            Assert.AreEqual(string.Empty, sqlDateString);
        }

        [Test]
        public void TryConvertToSqlTimeString_CorrectFormat_CorrectResult()
        {
            var result = Converter.TryConvertToSqlTimeString("10:58:41 PM", out var sqlTimeString, out var timeSpan);

            Assert.IsTrue(result);
            Assert.IsNotNull(timeSpan);
            Assert.AreEqual(new TimeSpan(22, 58, 41), timeSpan);
            Assert.AreEqual("22:58:41", sqlTimeString);
        }

        [Test]
        public void TryConvertToSqlTimeString_IncorrectFormat_EmptyResult()
        {
            var result = Converter.TryConvertToSqlTimeString("22:58 PM", out var sqlTimeString, out var timeSpan, "h:mm:ss tt");

            Assert.IsFalse(result);
            Assert.IsNull(timeSpan);
            Assert.AreEqual(string.Empty, sqlTimeString);
        }
    }
}
