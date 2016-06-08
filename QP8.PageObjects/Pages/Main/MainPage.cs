using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using QP8.PageObjects.Components;
using SeleniumExtension.Support.PageObjects.Attributes.Implementation;

namespace QP8.PageObjects.Pages.Main
{
    public class MainPage : BasePage
    {
        [By(How.CssSelector, "#header")]
        public HeaderComponent Header;

        [By(How.CssSelector, "#tree")]
        public TreeComponent Tree;

        public MainPage(IWebDriver webDriver) 
            : base(webDriver)
        {
        }
    }
}
