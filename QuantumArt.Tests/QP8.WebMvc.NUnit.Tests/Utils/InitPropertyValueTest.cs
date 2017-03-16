using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.Utils
{
    /// <summary>
    /// Summary description for InitPropertyValueTest
    /// </summary>
    [TestClass]
    public class InitPropertyValueTest
    {
        private readonly Func<int> _initializer;
        private readonly Func<int, int> _customSetter;

        private const int InitializerResult = 100;
        private const int CustomSetterResult = 200;

        public InitPropertyValueTest()
        {
            _initializer = () => InitializerResult;
            _customSetter = (v) => CustomSetterResult;
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetterSetterCheck_InitializerIsDefinedAndCustomSetterIsNull_CorrectResult()
        {
            var value = new InitPropertyValue<int>(_initializer);
            Assert.AreEqual(InitializerResult, value.Value);

            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(newValue, value.Value);
        }

        [TestMethod]
        public void GetterSetterCheck_InitializerAndCustomSetterIsDefined_CorrectResult()
        {
            var value = new InitPropertyValue<int>(_initializer, _customSetter);
            Assert.AreEqual(InitializerResult, value.Value);

            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(CustomSetterResult, value.Value);
        }

        [TestMethod]
        public void GetterSetterCheck_InitializerAndCustomSetterIsUndefined_CorrectResult()
        {
            var value = new InitPropertyValue<int>();

            // нет инициализации - значение по умолчанию
            Assert.AreEqual(default(int), value.Value);

            // Устанавливаем initializer
            value.Initializer = _initializer;

            Assert.AreEqual(InitializerResult, value.Value);

            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(newValue, value.Value);
        }

        [TestMethod]
        public void GetterSetterCheck_InitializerAndCustomSetterDefineLater_CorrectResult()
        {
            var value = new InitPropertyValue<int>();

            // нет инициализации - значение по умолчанию
            Assert.AreEqual(default(int), value.Value);

            // Проверка get/set
            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(newValue, value.Value);

            // Устанавливаем initializer
            value.Initializer = _initializer;

            // Так как уже был set, то initializer не применяется
            Assert.AreEqual(newValue, value.Value);
        }
    }
}
