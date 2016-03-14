using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Security;
using Quantumart.QP8.Configuration;


namespace TestProject
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ConfigTest
    {
        public ConfigTest()
        {
            //
            // TODO: Add constructor logic here
            //
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

		//[TestMethod]
		//public void CheckDefaultCustomerCode()
		//{
		//	Assert.IsTrue(QPContext.CheckCustomerCode(Default.CustomerCode));
		//}

		//[TestMethod]
		//public void CheckDefaultCustomerCodeCaseSensitive()
		//{
		//	Assert.IsFalse(QPContext.CheckCustomerCode(Default.CustomerCode.ToUpperInvariant()));
		//}

		//[TestMethod]
		//public void CheckAuthenticate()
		//{
		//    int errCode = 0;
		//    string message;
		//    LogOnCredentials data = new LogOnCredentials { UserName = Default.User, Password = Default.Password, CustomerCode = Default.CustomerCode, UseAutoLogin = false };
		//    QPUser user = QPContext.Authenticate(data, ref errCode, out message);
		//    Assert.IsNotNull(user);
		//    Assert.AreEqual(0, errCode);
		//}

		//[TestMethod]
		//public void TempDirectory()
		//{
		//    Assert.AreEqual(@"c:\temp\", QPConfiguration.ConfigVariable(Config.TempKey).ToLowerInvariant());
		//}
    }
}
