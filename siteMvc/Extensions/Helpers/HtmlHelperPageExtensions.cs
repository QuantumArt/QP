using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public static IHtmlContent PrepareInitScript(this IHtmlHelper html, ViewModel model)
        {
            return new HtmlString($"<script>var {model.ContextObjectName} = new Quantumart.QP8.BackendDocumentContext({model.MainComponentParameters.ToJson(Formatting.None)}, {model.MainComponentOptions.ToJson(Formatting.None)});</script>");
        }

        /// <summary>
        /// Выполняет JS-код инициализации страницы
        /// </summary>
        public static IHtmlContent RunInitScript(this IHtmlHelper html, ViewModel model)
        {
            return new HtmlString($"<script>{model.ContextObjectName}.init();</script>");
        }

        /// <summary>
        /// Генерирует и выполняет JS-код инициализации страницы
        /// </summary>
        public static IHtmlContent PrepareAndRunInitScript(this IHtmlHelper html, ViewModel model)
        {
            var content = new HtmlContentBuilder();
            content.AppendHtml(html.PrepareInitScript(model));
            content.AppendHtml(html.RunInitScript(model));
            return content;
        }

        /// <summary>
        /// Подменяет контекст у скрипта
        /// </summary>
        public static IHtmlContent CustomScript(this IHtmlHelper html, string script, string contextObjectName)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return HtmlString.Empty;
            }
            return new HtmlString($"<script>{script.Replace("QP_CURRENT_CONTEXT", contextObjectName)}</script>");
        }
    }
}
