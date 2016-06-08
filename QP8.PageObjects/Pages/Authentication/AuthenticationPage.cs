using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using QP8.PageObjects.Elements;
using SeleniumExtension.Support.PageObjects;
using SeleniumExtension.Support.PageObjects.Attributes.Implementation;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.PageObjects.Pages.Authentication
{
    public class AuthenticationPage : Page
    {
        [By(How.XPath, ".//dd[input[@id='UserName']]")]
        public Input Login;

        [By(How.XPath, ".//dd[input[@id='Password']]")]
        public Input Password;

        [By(How.XPath, ".//dd[input[@id='CustomerCode']]")]
        public Input CustomerCode;

        [By(How.XPath, ".//dd[input[@id='Login']]")]
        public Button Submit;

        [By(How.XPath, ".//dd[input[@id='Login']]/div/ul")]
        [ItemsBy(How.XPath, ".//dd[input[@id='Login']]/div/ul/li")]
        public ElementsList<Element> ValidationErrors;

        public AuthenticationPage(IWebDriver webDriver) 
            : base(webDriver)
        {
        }

        public void Authenticate(string login, string password, string customerCode)
        {
            Login.SendKeys(login);
            Password.SendKeys(password);
            CustomerCode.SendKeys(customerCode);
            Submit.Click();
        }
    }
}
