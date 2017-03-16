using System;
using System.Collections.Generic;
using System.Linq;
using AllureCSharpCommons.Attributes;
using NUnit.Framework;
using Nunit3AllureAdapter;
using OpenQA.Selenium;
using QP8.UI.Tests.Configuration;
using QP8.UI.Tests.PageObjects.Elements;
using QP8.UI.Tests.PageObjects.Pages.Authentication;
using QP8.UI.Tests.TestsData;

namespace QP8.UI.Tests.Tests.Authentication
{
    [AllureTitle(Title)]
    [AllureFeatures(Feature.Authentication)]
    public class AuthenticationTests : BaseTest
    {
        private const string Title = "Authentication form tests";

        protected void AuthenticationSteps(AuthenticationPage page, string login, string password, string customerCode)
        {
            Step($"Opening {Config.Tests.BackendUrl}", () =>
            {
                Driver.Url = Config.Tests.BackendUrl;
            });

            Step("Authentication", () =>
            {
                MakeAttachment(string.Format("Login: {1}{0}Password: {2}{0}CustomerCodeInput: {3}{0}", Environment.NewLine, login, password, customerCode), "Authentication data", TextType.textPlain);

                page.Login.SendKeys(login);
                page.Password.SendKeys(password);

                if (Config.Tests.BackendCustomerCodeFieldIsDropdown)
                {
                    page.CustomerCodeSelect.SelectByText(customerCode);
                }
                else
                {
                    page.CustomerCodeInput.SendKeys(customerCode);
                }

                page.Submit.Click();
            });
        }

        protected void CheckValidationSteps(AuthenticationPage page, Input fieldToCheck, string fieldToCheckName, string validationErrorText)
        {
            Step("Check validation", () =>
            {
                Assert.That(fieldToCheck.Valid, Is.False, $"{fieldToCheckName} input field is not highlighted with red");
                Assert.That(fieldToCheck.ValidationText, Is.EqualTo(validationErrorText), $"{fieldToCheckName} input field validation text is incorrect");
                Assert.That(page.Password.Text, Is.Empty, "Password input field is not empty");
                Assert.That(page.ValidationErrors.Count(), Is.EqualTo(1), "Validation errors miscount");
                Assert.That(page.ValidationErrors[0].Text, Is.EqualTo(validationErrorText), "Incorrect validation error text");
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
            {
                return;
            }

            foreach (var error in jsErrors)
            {
                MakeAttachment(error.Message, "JavaScript error", TextType.textPlain);
            }

            Assert.That(jsErrors, Is.Empty, "Found JavaScript errors");
        }
    }
}
