using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtension.Support.PageObjects.Elements;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.FunctionalTests.PageObjects.Elements
{
    /// <summary>
    /// Объектная модель выпадающего списка<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;select&gt; 
    ///         &lt;option value=&quot;option_value&quot;&gt;  
    ///      &lt;/select&gt;
    /// </code>
    /// </summary>
    public class Select : Element, IEnabled
    {
        protected SelectElement SelectElement
        {
            get { return new SelectElement(ProxyWebElement); }
        }

        /// <summary>
        /// Проверка доступности выпадающего списка
        /// </summary>
        /// <remarks>
        /// Выполняется путём проверки отсутствия атрибута 'disabled'<br/>
        /// и вызова свойства 'IWebElement.Enabled'
        /// </remarks>
        public bool Enabled
        {
            get { return string.IsNullOrEmpty(GetAttribute("disabled")) && ProxyWebElement.Enabled; }
        }

        /// <summary> 
        /// Проверка поддержки множественного выбора
        /// </summary>
        /// <remarks>
        /// Выполняется путём проверки наличия атрибута 'multiple'
        /// </remarks>
        public bool IsMultiple
        {
            get { return !string.IsNullOrEmpty(GetAttribute("multiple")); }
        }

        /// <summary>
        /// Получение элементов выпадающего списка
        /// </summary>
        public IReadOnlyCollection<Element> Options
        {
            get { return SelectElement.Options.Select(option => new Element(option, WebDriver)).ToList(); }
        }

        /// <summary>
        /// Получение первого выбранного элемента
        /// </summary>
        public Element SelectedOption
        {
            get
            {
                var selectedOption = SelectElement.Options.FirstOrDefault(option => option.Selected);
                return selectedOption != null ? new Element(selectedOption, WebDriver) : default(Element);
            }
        }

        /// <summary>
        /// Получение всех выбранных элементов
        /// </summary>
        public IReadOnlyCollection<Element> AllSelectedOptions
        {
            get
            {
                return SelectElement.Options.Where(option => option.Selected)
                                            .Select(option => new Element(option, WebDriver)).ToList();
            }
        }

        public Select(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }

        /// <summary>
        /// Выбор элемента по тексту
        /// </summary>
        /// <param name="text">Текст элемента</param>
        public void SelectByText(string text)
        {
            SelectElement.SelectByText(text);
        }

        /// <summary>
        /// Выбор элемента по атрибуту 'value'
        /// </summary>
        /// <param name="value">Значение атрибута 'value'</param>
        public void SelectByValue(string value)
        {
            SelectElement.SelectByValue(value);
        }

        /// <summary>
        /// Выбор элемента по индексу
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        public void SelectByIndex(int index)
        {
            SelectElement.SelectByIndex(index);
        }

        /// <summary>
        /// Снятия флага с выбранного элемента по тексту
        /// </summary>
        /// <param name="text">Текст элемента</param>
        public void DeselectByText(string text)
        {
            SelectElement.DeselectByText(text);
        }

        /// <summary>
        /// Снятие флага с выбранного элемента по атрибуту 'value'
        /// </summary>
        /// <param name="value">Значение атрибута 'value'</param>
        public void DeselectByValue(string value)
        {
            SelectElement.DeselectByValue(value);
        }

        /// <summary>
        /// Снятие флага с выбранного элемента по индексу
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        public void DeselectByIndex(int index)
        {
            SelectElement.DeselectByIndex(index);
        }
        
        /// <summary>
        /// Снятие флагов со всех выбранных элементов
        /// </summary>
        public void DeselectAll()
        {
            SelectElement.DeselectAll();
        }
    }
}
