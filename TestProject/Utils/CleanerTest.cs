using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;

namespace WebMvc.Test.Utils
{
    [TestClass]
    public class CleanerTest
    {
        [TestMethod]
        public void ToSafeSqlLikeCondition_NullInput_NullResult()
        {
            Assert.IsNull(Cleaner.ToSafeSqlLikeCondition(null));
        }

        [TestMethod]
        public void ToSafeSqlLikeCondition_SafeInput_SafeResult()
        {
            Assert.AreEqual("test", Cleaner.ToSafeSqlLikeCondition("test"));
        }

        [TestMethod]
        public void ToSafeSqlLikeCondition_UnsafeInput_SafeResult()
        {
            Assert.AreEqual("'' [[] [%] [_]",
                Cleaner.ToSafeSqlLikeCondition("' [ % _"));
        }


        [TestMethod]
        public void ToSafeSqlString_NullInput_NullResult()
        {
            Assert.IsNull(Cleaner.ToSafeSqlString(null));
        }

        [TestMethod]
        public void ToSafeSqlString_SafeInput_SafeResult()
        {
            Assert.AreEqual("test", Cleaner.ToSafeSqlString("test"));
        }

        [TestMethod]
        public void ToSafeSqlString_UnsafeInput_SafeResult()
        {
            Assert.AreEqual("''test''",
                Cleaner.ToSafeSqlString("'test'"));
        }


    }
}
