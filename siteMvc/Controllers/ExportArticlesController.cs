using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.AspNet.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
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
    public class ExportArticlesController : QPController
    {
        private readonly IMultistepActionService _service;
        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public ExportArticlesController(IMultistepActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult PreSettings(int parentId, int id) => Json(_service.MultistepActionSettings(parentId, id, null));

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult Settings(string tabId, int parentId, int id) => JsonHtml($"{FolderForTemplate}/ExportTemplate", new ExportViewModel
        {
            ContentId = id
        });

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal) => Json(_service.Setup(parentId, id, boundToExternal));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public JsonCamelCaseResult<JSendResponse> SetupWithParams(int parentId, int id, ExportViewModel model)
        {
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
                settings.FieldIdsToExpand = model.FieldsToExpand.ToArray();
            }

            _service.SetupWithParams(parentId, id, settings);
            return new JSendResponse { Status = JSendStatus.Success };
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step) => Json(_service.Step(stage, step));

        [HttpPost]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
