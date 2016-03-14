using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;

namespace WebMvc.Test.Utils
{
	/// <summary>
	/// Summary description for InitPropertyValueTest
	/// </summary>
	[TestClass]
	public class InitPropertyValueTest
	{
		private Func<int> initializer;
		private Func<int, int> customSetter;

		private static readonly int INITIALIZER_RESULT = 100;
		private static readonly int CUSTOM_SETTER_RESULT = 200;

		public InitPropertyValueTest()
		{
			initializer = () => INITIALIZER_RESULT;
			customSetter = (v) => CUSTOM_SETTER_RESULT;
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

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
			InitPropertyValue<int> value = new InitPropertyValue<int>(initializer);

			Assert.AreEqual(INITIALIZER_RESULT, value.Value);

			int newValue = 3 * INITIALIZER_RESULT;
			value.Value = newValue;
			Assert.AreEqual(newValue, value.Value);
		}

		[TestMethod]
		public void GetterSetterCheck_InitializerAndCustomSetterIsDefined_CorrectResult()
		{
			InitPropertyValue<int> value = new InitPropertyValue<int>(initializer, customSetter);

			Assert.AreEqual(INITIALIZER_RESULT, value.Value);

			int newValue = 3 * INITIALIZER_RESULT;
			value.Value = newValue;
			Assert.AreEqual(CUSTOM_SETTER_RESULT, value.Value);
		}

		[TestMethod]
		public void GetterSetterCheck_InitializerAndCustomSetterIsUndefined_CorrectResult()
		{
			InitPropertyValue<int> value = new InitPropertyValue<int>();

			// нет инициализации - значение по умолчанию
			Assert.AreEqual(default(int), value.Value);

			// Устанавливаем initializer
			value.Initializer = initializer;		
	
			Assert.AreEqual(INITIALIZER_RESULT, value.Value);

			int newValue = 3 * INITIALIZER_RESULT;
			value.Value = newValue;
			Assert.AreEqual(newValue, value.Value);
		}

		[TestMethod]
		public void GetterSetterCheck_InitializerAndCustomSetterDefineLater_CorrectResult()
		{
			InitPropertyValue<int> value = new InitPropertyValue<int>();

			// нет инициализации - значение по умолчанию
			Assert.AreEqual(default(int), value.Value);

			// Проверка get/set 
			int newValue = 3 * INITIALIZER_RESULT;
			value.Value = newValue;
			Assert.AreEqual(newValue, value.Value);

			// Устанавливаем initializer
			value.Initializer = initializer;
			// Так как уже был set, то initializer не применяется
			Assert.AreEqual(newValue, value.Value);
		}
	}
}
