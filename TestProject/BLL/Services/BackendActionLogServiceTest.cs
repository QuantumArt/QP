using Quantumart.QP8.BLL.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Quantumart.QP8.BLL.Services.Audit;

namespace WebMvc.Test.BLL.Services
{
    
    
    /// <summary>
    ///This is a test class for BackendActionLogServiceTest and is intended
    ///to contain all BackendActionLogServiceTest Unit Tests
    ///</summary>
	[TestClass()]
	public class BackendActionLogServiceTest
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


        //[TestMethod()]
        //[DeploymentItem("Quantumart.QP8.BLL.dll")]
        //public void NoParametersAreDefined_CreateFilterTest()
        //{
        //    string actionTypeCode = null;
        //    string entityTypeCode = null;
        //    string entityStringId = null;			
        //    string actual;
        //    actual = BackendActionLogService_Accessor.CreateFilter(actionTypeCode, entityTypeCode, entityStringId);
        //    Assert.IsNull(actual);
        //}

        //[TestMethod()]
        //[DeploymentItem("Quantumart.QP8.BLL.dll")]
        //public void  AllFilterParametersAreDefined_CreateFilterTest()
        //{
        //    string actionTypeCode = "action_type_code"; 
        //    string entityTypeCode = "entity_type_code"; 
        //    string entityStringId = "entity_string_id";
        //    string expected = @"[ActionTypeCode] = N'action_type_code' AND [EntityTypeCode] = N'entity_type_code' AND [EntityStringId] LIKE N'%entity_string_id%'"; 
        //    string actual;
        //    actual = BackendActionLogService_Accessor.CreateFilter(actionTypeCode, entityTypeCode, entityStringId);
        //    Assert.AreEqual(expected, actual);			
        //}

        //[TestMethod()]
        //[DeploymentItem("Quantumart.QP8.BLL.dll")]
        //public void ActionTypeCodeAndEntityStringIdAreDefined_CreateFilterTest()
        //{
        //    string actionTypeCode = "action_type_code";
        //    string entityTypeCode = null;
        //    string entityStringId = "entity_string_id";
        //    string expected = @"[ActionTypeCode] = N'action_type_code' AND [EntityStringId] LIKE N'%entity_string_id%'";
        //    string actual;
        //    actual = BackendActionLogService_Accessor.CreateFilter(actionTypeCode, entityTypeCode, entityStringId);
        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod()]
        //[DeploymentItem("Quantumart.QP8.BLL.dll")]
        //public void OnlyEntityTypeCodeIsDefined_CreateFilterTest()
        //{
        //    string actionTypeCode = null;
        //    string entityTypeCode = "entity_type_code";
        //    string entityStringId = null;
        //    string expected = @"[EntityTypeCode] = N'entity_type_code'";
        //    string actual;
        //    actual = BackendActionLogService_Accessor.CreateFilter(actionTypeCode, entityTypeCode, entityStringId);
        //    Assert.AreEqual(expected, actual);
        //}

	}
}
