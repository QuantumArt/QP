using System;
using System.Collections.Generic;
using System.Linq;
using AllureCSharpCommons.Attributes;
using Nunit3AllureAdapter;
using NUnit.Framework;
using OpenQA.Selenium;
using QP8.FunctionalTests.Configuration;
using QP8.FunctionalTests.PageObjects.Elements;
using QP8.FunctionalTests.PageObjects.Pages.Authentication;
using QP8.FunctionalTests.TestsData;

namespace QP8.FunctionalTests.Tests.Authentication
{
    [AllureTitle(Title)]
    [AllureFeatures(Feature.Authentication)]
    public class AuthenticationTests : BaseTest
    {
        private const string Title = "Authentication form tests";

        #region common steps

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
                               "Authentication data", TextType.textPlain);

                page.Authenticate(login, password, customerCode);
            });
        }

        protected void CheckValidationSteps(AuthenticationPage page, Input fieldToCheck,
                                            string fieldToCheckName, string validationErrorText)
        {
            Step("Check validation", () =>
            {
                Assert.That(fieldToCheck.Valid, Is.False,
                    string.Format("{0} input field is not highlighted with red", fieldToCheckName));

                Assert.That(fieldToCheck.ValidationText, Is.EqualTo(validationErrorText),
                    string.Format("{0} input field validation text is incorrect", fieldToCheckName));

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
    }
}
