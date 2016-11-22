using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ExportSelectedArticlesController : QPController
    {
        private readonly IMultistepActionService _service;
        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public ExportSelectedArticlesController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult PreSettings(int parentId, int[] IDs)
        {
            var prms = _service.MultistepActionSettings(parentId, 0, IDs);
            return Json(prms);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Settings(string tabId, int parentId, int[] IDs)
        {
            var model = new ExportViewModel
            {
                ContentId = parentId,
                Ids = IDs
            };

            var viewName = $"{FolderForTemplate}/ExportTemplate";
            return JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Setup(int parentId, int[] IDs, bool? boundToExternal)
        {
            var settings = _service.Setup(parentId, 0, IDs, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult SetupWithParams(int parentId, int[] IDs, FormCollection collection)
        {
            var model = new ExportViewModel();

            TryUpdateModel(model);
            var settings = new ExportSettings(parentId, null)
            {
                Culture = MultistepActionHelper.GetCulture(model.Culture),
                Delimiter = MultistepActionHelper.GetDelimiter(model.Delimiter),
                Encoding = MultistepActionHelper.GetEncoding(model.Encoding),
                LineSeparator = MultistepActionHelper.GetLineSeparator(model.LineSeparator),
                AllFields = model.AllFields,
                OrderByField = model.OrderByField
            };

            if (!settings.AllFields)
            {
                settings.CustomFieldIds = model.CustomFields.ToArray();
                settings.ExcludeSystemFields = model.ExcludeSystemFields;
            }

            settings.FieldIdsToExpand = model.FieldsToExpand.ToArray();
            _service.SetupWithParams(parentId, IDs, settings);
            return Json(string.Empty);
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            var stepResult = _service.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
