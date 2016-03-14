using Quantumart.QP8.BLL.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebMvc.Test.BLL.Helpers
{
    
    
    /// <summary>
    ///This is a test class for MultistepActionHelperTest and is intended
    ///to contain all MultistepActionHelperTest Unit Tests
    ///</summary>
	[TestClass()]
	public class MultistepActionHelperTest
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


		/// <summary>
		///A test for GetStepCount
		///</summary>
		[TestMethod()]
		public void GetStepCountTest()
		{
			Assert.AreEqual(0, MultistepActionHelper.GetStepCount(0, 20));
			Assert.AreEqual(1, MultistepActionHelper.GetStepCount(1, 20));
			Assert.AreEqual(1, MultistepActionHelper.GetStepCount(19, 20));
			Assert.AreEqual(1, MultistepActionHelper.GetStepCount(20, 20));
			Assert.AreEqual(2, MultistepActionHelper.GetStepCount(21, 20));
			Assert.AreEqual(5, MultistepActionHelper.GetStepCount(100, 20));
			Assert.AreEqual(6, MultistepActionHelper.GetStepCount(101, 20));
		}
	}
}
