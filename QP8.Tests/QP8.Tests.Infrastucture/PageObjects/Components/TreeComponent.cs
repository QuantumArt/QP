using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.Tests.Infrastucture.PageObjects.Components
{
    public class TreeComponent : Component
    {
        public TreeComponent(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }
    }
}
