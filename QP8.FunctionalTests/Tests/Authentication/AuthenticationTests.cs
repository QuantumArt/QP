using System;
using System.Collections.Generic;
using System.Linq;
using AllureCSharpCommons.AllureModel;
using AllureCSharpCommons.Attributes;
using Nunit3AllureAdapter;
using NUnit.Framework;
using OpenQA.Selenium;
using QP8.FunctionalTests.Configuration;
using QP8.FunctionalTests.TestsData;
using QP8.FunctionalTests.TestsData.Authentication;
using QP8.PageObjects.Elements;
using QP8.PageObjects.Pages.Authentication;
using QP8.PageObjects.Pages.Main;

namespace QP8.FunctionalTests.Tests.Authentication
{
    [TestFixture]
    [AllureTitle(Title)]
    [AllureFeatures(Feature.Authentication)]
    public class AuthenticationTests : BaseTest
    {
        public const string Title = "Authentication form tests";

        #region valid data

        [AllureTest]
        [AllureTitle("Authentication with valid data")]
        [AllureDescription("Valid data", descriptiontype.html)]
        [AllureStories(Story.Positive)]
        [AllureSeverity(severitylevel.critical)]
        public void AuthenticationWithValidData()
        {
            var login = Config.QP8BackendLogin;
            var password = Config.QP8BackendPassword;
            var customerCode = Config.QP8BackendCustomerCode;

            AuthenticationSteps(new AuthenticationPage(Driver), login, password, customerCode);

            Step("Authentication check", () =>
            {
                var mainPage = new MainPage(Driver);
                Assert.That(mainPage.Header.UserName.Text, Is.EqualTo(login), "Incorrect user name");
                Assert.That(mainPage.Tree.Displayed, Is.True, "Menu tree is not displayed");
            });

            CheckJavaScriptErrors();
        }

        #endregion

        #region invalid login

        [AllureTest]
        [AllureTitle("Authentication with invalid login")]
        [AllureDescription("Invalid login", descriptiontype.html)]
        [AllureStories(Story.Negative)]
        [AllureSeverity(severitylevel.normal)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidLogin")]
        public void AuthenticationWithInvalidLogin(string login)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, login, Config.QP8BackendPassword, Config.QP8BackendCustomerCode);
            CheckValidationSteps(page, page.Login, "Login", "Your account does not exist!");
            CheckJavaScriptErrors();
        }

        #endregion
    }
}
