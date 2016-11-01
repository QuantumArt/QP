using System.Linq;
using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements;

namespace QP8.Tests.Infrastucture.PageObjects.Elements
{
    /// <summary>
    /// Объектная модель поля ввода<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;dd class=&quot;field&quot;&gt;
    ///         &lt;input class=&quot;input-validation-error textbox&quot; type=&quot;text&quot;&gt;
    ///         &lt;em class=&quot;validators&quot;&gt;
    ///             &lt;span class=&quot;field-validation-error&quot; &gt;
    ///         &lt;/em&gt;
    ///      &lt;/dd&gt;
    /// </code>
    /// </summary>
    public class Input : InputBasedElement, IEditable, IEnabled, IFocused
    {
        /// <summary>
        /// Проверка доступности поля ввода
        /// </summary>
        /// <remarks>
        /// Выполняется путём проверки отсутствия атрибута 'disabled'<br/>
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

        /// <summary>
        /// Проверка наличия фокуса на поле ввода
        /// </summary>
        /// <remarks>
        /// Выполняется путём получения активного элемента на странице и его сравнения с полем ввода
        /// </remarks>
        public bool Focused => ProxyWebElement.Equals(WebDriver.SwitchTo().ActiveElement());

        /// <summary>
        /// Проверка валидации поля
        /// </summary>
        /// <remarks>
        /// Выполняется путём проверки значения атрибута 'class' дочернего элемента 'input'<br/>
        /// на отсутствие параметра 'input-validation-error' или отсутствия текста валидации
        /// </remarks>
        public bool Valid
        {
            get
            {
                var cssClass = ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("class");

                return !string.IsNullOrEmpty(cssClass)
                    ? !cssClass.Contains("input-validation-error")
                    : string.IsNullOrEmpty(ValidationText);
            }
        }

        /// <summary>
        /// Получение текста валидации
        /// </summary>
        /// <remarks>
        /// Выполняется путём поиска дочернего элемента 'span' с атрибутом 'class="field-validation-error"'<br/>
        /// и получения его текста
        /// </remarks>
        public string ValidationText
        {
            get
            {
                var errors = ProxyWebElement.FindElements(By.CssSelector("span.field-validation-error"));
                return errors.Any() && errors.First().Displayed ? errors.First().Text : string.Empty;
            }
        }

        public Input(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }

        /// <summary>
        /// Удаление текста из поля ввода
        /// </summary>
        /// <remarks>
        /// Выполяется путём вызова метода 'IWebElement.Clear()' дочернего элемента 'input'
        /// </remarks>
        public void Clear()
        {
            ProxyWebElement.FindElement(By.CssSelector("input")).Clear();
        }

        /// <summary>
        /// Передача текста полю ввода
        /// </summary>
        /// <remarks>
        /// Выполяется путём вызова метода 'IWebElement.SendKeys(string)' дочергено элемента 'input'
        /// </remarks>
        /// <param name="text">Текст для передачи</param>
        public void SendKeys(string text)
        {
            ProxyWebElement.FindElement(By.CssSelector("input")).SendKeys(text);
        }
    }
}
