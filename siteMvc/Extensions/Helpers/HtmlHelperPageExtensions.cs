using System.Web.Mvc;
using Newtonsoft.Json;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperPageExtensions
    {
        /// <summary>
        /// Генерирует JS-код инициализации страницы
        /// </summary>
        public static MvcHtmlString PrepareInitScript(this HtmlHelper html, ViewModel model) =>
            MvcHtmlString.Create($"<script>var {model.ContextObjectName} = new Quantumart.QP8.BackendDocumentContext({model.MainComponentParameters.ToJson(Formatting.None)}, {model.MainComponentOptions.ToJson(Formatting.None)});</script>");

        /// <summary>
        /// Выполняет JS-код инициализации страницы
        /// </summary>
        public static MvcHtmlString RunInitScript(this HtmlHelper html, ViewModel model) =>
            MvcHtmlString.Create($"<script>{model.ContextObjectName}.init();</script>");

        /// <summary>
        /// Генерирует и выполняет JS-код инициализации страницы
        /// </summary>
        public static MvcHtmlString PrepareAndRunInitScript(this HtmlHelper html, ViewModel model) =>
            MvcHtmlString.Create(html.PrepareInitScript(model) + html.RunInitScript(model).ToString());

        /// <summary>
        /// Подменяет контекст у скрипта
        /// </summary>
        public static MvcHtmlString CustomScript(this HtmlHelper html, string script, string contextObjectName) =>
            MvcHtmlString.Create(string.IsNullOrWhiteSpace(script) ? string.Empty : $"<script>{script.Replace("QP_CURRENT_CONTEXT", contextObjectName)}</script>");
    }
}
