using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.UI.Tests.PageObjects.Elements
{
    /// <summary>
    /// Объектная модель элемента на основе 'input'<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;input type=&quot;text&quot;&gt;
    /// </code>
    /// </summary>
    public class InputBasedElement : Element
    {
        /// <summary>
        /// Получение текста элемента
        /// </summary>
        /// <remarks>
        /// Выполняется путём поиска дочернего элемента 'input' и получения его текста
        /// </remarks>
        public new string Text => ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("value");

        public InputBasedElement(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }
    }
}
