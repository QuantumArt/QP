using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quantumart.QP8.BLL.Repository;
using System.Globalization;
using Quantumart.QP8.BLL.Helpers;

namespace WebMvc.Test.BLL.Services
{
	[TestClass]
	public class ApplicationInfoServiceTest
	{		
		[TestMethod]
		[DeploymentItem("TestData\\fix_dbo.testsample.sql")]
		public void GetCurrentFixDboVersionTest()
		{
			string expected = "7.9.1.0";
			string actual = new ApplicationInfoHelper().GetCurrentFixDboVersion("fix_dbo.testsample.sql");

			Assert.AreEqual(expected, actual, true, CultureInfo.InvariantCulture);
		}

		[TestMethod]
		public void VersionsEqualTest_VersionsAreEqual_ReturnTrue()
		{
			bool actual = new ApplicationInfoHelper().VersionsEqual("7.9.1.0", "7.9.1.0");
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public void VersionsEqualTest_VersionsAreNotEqual_ReturnFalse()
		{
			bool actual = new ApplicationInfoHelper().VersionsEqual("7.9.1.1", "7.9.1.0");
			Assert.IsFalse(actual);
		}
	}
}
