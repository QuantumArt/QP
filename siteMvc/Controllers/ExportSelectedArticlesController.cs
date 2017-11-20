using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.AspNet.ActionResults;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

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
        public ActionResult PreSettings(int parentId, int[] IDs) => new JsonNetResult(_service.MultistepActionSettings(parentId, 0, IDs));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Settings(string tabId, int parentId, int[] IDs) => JsonHtml($"{FolderForTemplate}/ExportTemplate", new ExportViewModel
        {
            ContentId = parentId,
            Ids = IDs
        });

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Setup(int parentId, int[] IDs, bool? boundToExternal) => new JsonNetResult(_service.Setup(parentId, 0, IDs, boundToExternal));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult SetupWithParams(int parentId, int[] IDs, FormCollection collection)
        {
            var model = new ExportViewModel();
            TryUpdateModel(model);

            var settings = new ExportSettings
            {
                Culture = ((CsvCulture)int.Parse(model.Culture)).Description(),
                Delimiter = char.Parse(((CsvDelimiter)int.Parse(model.Delimiter)).Description()),
                Encoding = ((CsvEncoding)int.Parse(model.Encoding)).Description(),
                LineSeparator = ((CsvLineSeparator)int.Parse(model.LineSeparator)).Description(),
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
        public ActionResult Step(int stage, int step) => new JsonNetResult(_service.Step(stage, step));

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
