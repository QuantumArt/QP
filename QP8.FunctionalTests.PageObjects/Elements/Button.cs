using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements;

namespace QP8.FunctionalTests.PageObjects.Elements
{
    /// <summary>
    /// Объёктная модель кнопки<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;input type=&quot;submit&quot;&gt;
    /// </code>
    /// </summary>
    public class Button : InputBasedElement, IClickable, IEnabled
    {
        /// <summary>
        /// Проверка доступности кнопки
        /// </summary>
        /// <remarks>
        /// Выполняется путём проверки отсутствия атрибута 'disabled' дочернего элемента 'input'<br/>
        /// и вызова свойства 'IWebElement.Enabled'
        /// </remarks>
        public bool Enabled
        {
            get
            {
                var disabled = ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("disabled");
                return string.IsNullOrEmpty(disabled) && ProxyWebElement.Enabled;
            }
        }

        public Button(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }

        /// <summary>
        /// Клик по кнопке
        /// </summary>
        /// <remarks>
        /// Выполняется путём вызова метода 'IWebElement.Click()' дочернего элемента 'input'
        /// </remarks>
        public void Click()
        {
            ProxyWebElement.FindElement(By.CssSelector("input")).Click();
        }
    }
}
