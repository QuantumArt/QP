using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.Tests.Infrastucture.PageObjects.Elements
{
    /// <summary>
    /// Объектная модель ссылки<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;a href=&quot;link_href&quot;&gt;link_text&lt;a/&gt;
    /// </code>
    /// </summary>
    public class Link : Element, IClickable, IHasHref
    {
        /// <summary>
        /// Получение ссылки
        /// </summary>
        /// <remarks>
        /// Выполняется путём получения значения атрибута 'href' с помощью метода<br/>
        /// 'IWebElement.GetAttribute(string attribute)'
        /// </remarks>
        public string Href => ProxyWebElement.GetAttribute("href");

        public Link(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }

        /// <summary>
        /// Клик по ссылке
        /// </summary>
        /// <remarks>
        /// Выполняется путём вызова метода 'IWebElement.Click()'
        /// </remarks>
        public void Click()
        {
            ProxyWebElement.Click();
        }
    }
}
