using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using QP8.Tests.Infrastucture.Infrastucture.Selenium.PageObjects.Elements;
using SeleniumExtension.Support.PageObjects.Attributes.Implementation;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.Tests.Infrastucture.Infrastucture.Selenium.PageObjects.Components
{
    public class HeaderComponent : Component
    {
        [By(How.CssSelector, "#communicationMessage>span.qpversion")]
        public Element QpVersion;

        [By(How.CssSelector, "#userInformation>span.userName")]
        public Element UserName;

        [By(How.CssSelector, "#userInformation>a.signOut")]
        public Link Exit;

        public HeaderComponent(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }
    }
}
