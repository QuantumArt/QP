using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using QP8.FunctionalTests.PageObjects.Elements;
using SeleniumExtension.Support.PageObjects;
using SeleniumExtension.Support.PageObjects.Attributes.Implementation;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.FunctionalTests.PageObjects.Pages.Authentication
{
    public class AuthenticationPage : Page
    {
        [By(How.XPath, ".//dd[input[@id='UserName']]")]
        public Input Login;

        [By(How.XPath, ".//dd[input[@id='Password']]")]
        public Input Password;

        [By(How.XPath, ".//dd[input[@id='CustomerCodeInput']]")]
        public Input CustomerCodeInput;

        [By(How.XPath, ".//select[@id='CustomerCode']")]
        public Select CustomerCodeSelect;

        [By(How.XPath, ".//dd[input[@id='Login']]")]
        public Button Submit;

        [By(How.XPath, ".//dd[input[@id='Login']]/div/ul")]
        [ItemsBy(How.XPath, ".//dd[input[@id='Login']]/div/ul/li")]
        public ElementsList<Element> ValidationErrors;

        public AuthenticationPage(IWebDriver webDriver)
            : base(webDriver)
        {
        }
    }
}
