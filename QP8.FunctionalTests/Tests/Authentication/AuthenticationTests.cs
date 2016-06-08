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

        #region commont steps

        protected void AuthenticationSteps(AuthenticationPage page, string login, string password, string customerCode)
        {
            Step(string.Format("Opening {0}", Config.QP8BackendUrl), () =>
            {
                Driver.Url = Config.QP8BackendUrl;
            });

            Step("Authentication", () =>
            {
                MakeAttachment(string.Format("Login: {1}{0}Password: {2}{0}CustomerCode: {3}{0}",
                                             Environment.NewLine, login, password, customerCode),
                               "authentication data", TextType.textPlain);

                page.Authenticate(login, password, customerCode);
            });
        }

        protected void CheckValidationSteps(AuthenticationPage page, Input inputFieldToCheck, string validationErrorText)
        {
            Step("Check validation", () =>
            {
                Assert.That(inputFieldToCheck.Valid, Is.False,
                    "Password input field is not highlighted with red");

                Assert.That(inputFieldToCheck.ValidationText, Is.EqualTo(validationErrorText),
                    "Incorrect password input field validation text");

                Assert.That(page.Password.Text, Is.Empty,
                    "Password input field is not empty");
                
                Assert.That(page.ValidationErrors.Count(), Is.EqualTo(1),
                    "Validation errors miscount");

                

                Assert.That(page.ValidationErrors[0].Text, Is.EqualTo(validationErrorText),
                    "Incorrect validation error text");
            });
        }

        protected void CheckJavaScriptErrors()
        {
            var errors = new List<string>
            {
                "SyntaxError",
                "EvalError",
                "ReferenceError",
                "RangeError",
                "TypeError",
                "URIError"
            };

            var logs = Driver.Manage().Logs.GetLog(LogType.Browser);
            var jsErrors = logs.Where(log => errors.Any(error => log.Message.Contains(error))).ToList();

            if (!jsErrors.Any())
                return;

            foreach (var error in jsErrors)
            {
                MakeAttachment(error.Message, "JavaScript error", TextType.textPlain);
            }

            Assert.That(jsErrors, Is.Empty, "Found JavaScript errors");
        }

        #endregion

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
            CheckValidationSteps(page, page.Login, "Your account does not exist!");
            CheckJavaScriptErrors();
        }

        #endregion

        #region invalid password

        [AllureTest]
        [AllureTitle("Authentication with invalid password")]
        [AllureDescription("Invalid password", descriptiontype.html)]
        [AllureStories(Story.Negative)]
        [AllureSeverity(severitylevel.normal)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidPassword")]
        public void AuthenticationWithInvalidPassword(string password)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.QP8BackendLogin, password, Config.QP8BackendCustomerCode);
            CheckValidationSteps(page, page.Password, "You entered wrong password!");
            CheckJavaScriptErrors();
        }

        #endregion

        #region invalid customer code

        [AllureTest]
        [AllureTitle("Authentication with invalid customer code")]
        [AllureDescription("Invalid customer code", descriptiontype.html)]
        [AllureStories(Story.Negative)]
        [AllureSeverity(severitylevel.normal)]
        [TestCaseSource(typeof(AuthenticationTestsData), "InvalidCustomerCode")]
        public void AuthenticationWithInvalidCustomerCode(string customerCode)
        {
            var page = new AuthenticationPage(Driver);

            AuthenticationSteps(page, Config.QP8BackendLogin, Config.QP8BackendPassword, customerCode);
            CheckValidationSteps(page, page.CustomerCode, "Customer code does not exist!");
            CheckJavaScriptErrors();
        }

        #endregion
    }
}
