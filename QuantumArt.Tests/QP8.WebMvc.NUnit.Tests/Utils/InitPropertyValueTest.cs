using System;
using NUnit.Framework;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.Utils
{
    [TestFixture]
    public class InitPropertyValueTest
    {
        private readonly Func<int> _initializer;
        private readonly Func<int, int> _customSetter;

        private const int InitializerResult = 100;
        private const int CustomSetterResult = 200;

        public InitPropertyValueTest()
        {
            _initializer = () => InitializerResult;
            _customSetter = v => CustomSetterResult;
        }

        public TestContext TestContext { get; set; }

        [Test]
        public void GetterSetterCheck_InitializerIsDefinedAndCustomSetterIsNull_CorrectResult()
        {
            var value = new InitPropertyValue<int>(_initializer);
            Assert.AreEqual(InitializerResult, value.Value);

            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(newValue, value.Value);
        }

        [Test]
        public void GetterSetterCheck_InitializerAndCustomSetterIsDefined_CorrectResult()
        {
            var value = new InitPropertyValue<int>(_initializer, _customSetter);
            Assert.AreEqual(InitializerResult, value.Value);

            const int newValue = 3 * InitializerResult;
            value.Value = newValue;
            Assert.AreEqual(CustomSetterResult, value.Value);
        }

        [Test]
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

        [Test]
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
