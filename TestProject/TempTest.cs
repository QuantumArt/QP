using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Microsoft.Practices.Unity;
using Moq;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ArticleScheduler;
using System.Threading;
using Quantumart.QP8.BLL.Services.ArticleScheduler;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WebMvc.Test
{
    /// <summary>
    /// Summary description for TempTest
    /// </summary>
    [TestClass]
    public class TempTest
    {
        public TempTest()
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
		//public void foo()
		//{
		//    var r = QPConfiguration.ConfigConnectionStrings("Test", new[] { "Publishing_R1", "kasdkasjdk" });
		//    r = QPConfiguration.ConfigConnectionStrings("Test", new[] { "Publishing_R1", "PUBLISHING", "kasdkasjdk" });
		//    r = QPConfiguration.ConfigConnectionStrings("Test", new[] {"sadsadsadsa" });
		//    r = QPConfiguration.ConfigConnectionStrings("Test", new string[] {});
		//    r = QPConfiguration.ConfigConnectionStrings("Test");
		//    r = null;
		//}
	
    }
}
