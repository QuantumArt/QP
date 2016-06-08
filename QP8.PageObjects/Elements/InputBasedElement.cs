using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.PageObjects.Elements
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
        /// Получение текста элемента<br/>
        /// Выполняется путём поиска дочернего элемента 'input' и получения его текста
        /// </summary>
        public new string Text
        {
            get
            {
                return ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("value");
            }
        }

        public InputBasedElement(IWebElement webElement, IWebDriver webDriver) 
            : base(webElement, webDriver)
        {
        }
    }
}
