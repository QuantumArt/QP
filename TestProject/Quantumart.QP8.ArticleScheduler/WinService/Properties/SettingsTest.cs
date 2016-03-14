using Quantumart.QP8.ArticleScheduler.WinService.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMvc.Test.WinService.Properties
{
    
    
    /// <summary>
    ///This is a test class for SettingsTest and is intended
    ///to contain all SettingsTest Unit Tests
    ///</summary>
	[TestClass()]
	public class SettingsTest
	{


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
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		
		[TestMethod()]
		public void SplitExceptCustomerCodesTest_ArgumentIsEmptyString_ReturnEmptyResult()
		{
			string exceptCustomerCodes = string.Empty; 			
			IEnumerable<string> actual;
			actual = Settings.SplitExceptCustomerCodes(exceptCustomerCodes);

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());			
		}

		[TestMethod()]
		public void SplitExceptCustomerCodesTest_ArgumentIsNull_ReturnEmptyResult()
		{			
			IEnumerable<string> actual;
			actual = Settings.SplitExceptCustomerCodes(null);

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod()]
		public void SplitExceptCustomerCodesTest_VariousArguments_ReturnCorrectResults()
		{			
			
			string exceptCustomerCodes = string.Empty;
			IEnumerable<string> expect = new[] { "c1", "c2", "c3" };
			IEnumerable<string> actual;

			Action testResult = () =>
			{
				string message = "Argument is: \"" + exceptCustomerCodes + "\"";
				actual = Settings.SplitExceptCustomerCodes(exceptCustomerCodes);
				Assert.IsNotNull(actual, message);
				Assert.AreEqual(expect.Count(), actual.Count(), message);
				Assert.IsFalse(expect.Except(actual).Any(), message);
			};

			exceptCustomerCodes = "c1;c2;c3";
			testResult();

			exceptCustomerCodes = "c1;c2;c3;";
			testResult();

			exceptCustomerCodes = ";c1;c2;c3;";
			testResult();

			exceptCustomerCodes = "  ;;c1;c2;c3;;  ";
			testResult();

			exceptCustomerCodes = " c1; c2;c3; ";
			testResult();

			exceptCustomerCodes = " c1;c2; c3 ";
			testResult();

			exceptCustomerCodes = " ; ;; c1;; c2;;;    c3 ; ;;";
			testResult();

			
			expect = new[] { "c1" };

			exceptCustomerCodes = "c1;";
			testResult();

			exceptCustomerCodes = " c1;  ";			
			testResult();

			exceptCustomerCodes = " c1  ";
			testResult();
		}		
	}
}
