using System;
using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VisualEditorConfigController : QPController
    {
        public JsonCamelCaseResult<JSendResponse> LoadVeConfig(int fieldId, int siteId)
        {
            var result = new JSendResponse { Status = JSendStatus.Success };

            VisualEditorStyleVm StyleMapFn(VisualEditorStyle entry) => new VisualEditorStyleVm
            {
                Name = entry.Name,
                Element = entry.Tag,
                Overrides = entry.OverridesTag,
                Styles = entry.StylesItems.Any() ? entry.StylesItems.ToDictionary(k => k.Name.Replace(' ', '_'), v => v.ItemValue) : null,
                Attributes = entry.AttributeItems.Any() ? entry.AttributeItems.ToDictionary(k => k.Name, v => v.ItemValue) : null
            };

            VisualEditorPluginVm PluginMapFn(VisualEditorPlugin entry) => new VisualEditorPluginVm
            {
                Name = entry.Name,
                Url = entry.Url
            };

            var veCommands = FieldService.GetResultVisualEditorCommands(fieldId, siteId).Where(n => n.On).ToList();
            var includedStyles = FieldService.GetResultStyles(fieldId, siteId).Where(ves => ves.On).OrderBy(ves => ves.Order).ToList();
            var model = new VisualEditorConfigVm
            {
                StylesSet = includedStyles.Where(ves => !ves.IsFormat).Select(StyleMapFn),
                FormatsSet = includedStyles.Where(ves => ves.IsFormat).Select(StyleMapFn).EmptyIfNull(),
                ExtraPlugins = VisualEditorHelpers.GetVisualEditorPlugins(veCommands).Select(PluginMapFn).EmptyIfNull(),
                Toolbar = VisualEditorHelpers.GenerateToolbar(veCommands)
            };

            if (fieldId != 0)
            {
                var field = FieldService.Read(fieldId);
                model.Language = field.VisualEditor.Language;
                model.DocType = field.VisualEditor.DocType;
                model.FullPage = field.VisualEditor.FullPage;
                model.EnterMode = field.VisualEditor.EnterMode;
                model.ShiftEnterMode = field.VisualEditor.ShiftEnterMode;
                model.UseEnglishQuotes = field.VisualEditor.UseEnglishQuotes;
                model.DisableListAutoWrap = field.VisualEditor.DisableListAutoWrap;
                model.Height = field.VisualEditor.Height;
                model.BodyClass = field.RootElementClass;
                model.ContentsCss = field.ExternalCssItems.Select(css => css.Url);
            }

            try
            {
                result.Data = model;
            }
            catch (Exception)
            {
                result.Status = JSendStatus.Error;
                result.Message = "Непредвиденная ошибка на сервере";
            }

            return result;
        }

        [HttpPost]
        public ActionResult AspellCheck(string text) => View(new AspellCheckVm(text));
    }
}
