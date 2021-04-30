using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ExportSelectedArchiveArticlesController : AuthQpController
    {
        private readonly IMultistepActionService _service;
        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public ExportSelectedArchiveArticlesController(ExportArticlesService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportSelectedArchiveArticles)]
        [BackendActionContext(ActionCode.ExportSelectedArchiveArticles)]
        public ActionResult PreSettings(int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return Json(_service.MultistepActionSettings(parentId, 0, model.Ids));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportSelectedArchiveArticles)]
        [BackendActionContext(ActionCode.ExportSelectedArchiveArticles)]
        public async Task<ActionResult> Settings(string tabId, int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return await JsonHtml($"{FolderForTemplate}/ExportTemplate", new ExportViewModel
            {
                ContentId = parentId,
                Ids = model.Ids,
                IsArchive = true
            });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportSelectedArchiveArticles)]
        [BackendActionContext(ActionCode.ExportSelectedArchiveArticles)]
        public ActionResult Setup(int parentId, [FromBody] SelectedItemsViewModel model, bool? boundToExternal)
        {
            return Json(_service.Setup(parentId, 0, model.Ids, boundToExternal, true));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportSelectedArchiveArticles)]
        [BackendActionContext(ActionCode.ExportSelectedArchiveArticles)]
        public JsonResult SetupWithParams(int parentId, [FromBody] ExportViewModel model)
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
                settings.CustomFieldIds = model.CustomFields.ToList();
                settings.ExcludeSystemFields = model.ExcludeSystemFields;
            }

            settings.IsArchive = true;
            settings.FieldIdsToExpand = model.FieldsToExpand.ToList();
            _service.SetupWithParams(parentId, model.Ids, settings);
            return JsonCamelCase(new JSendResponse { Status = JSendStatus.Success });
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step([FromBody] MultiStepActionViewModel model)
        {
            return Json(_service.Step(model.Stage, model.Step));
        }

        [HttpPost]
        public void TearDown(bool isError)
        {
            _service.TearDown();
        }
    }
}
