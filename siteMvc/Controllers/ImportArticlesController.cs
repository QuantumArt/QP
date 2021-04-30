using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;
using ContentService = Quantumart.QP8.BLL.Services.ContentServices.ContentService;
using DbService = Quantumart.QP8.BLL.Services.DbServices.DbService;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ImportArticlesController : AuthQpController
    {
        private readonly IMultistepActionService _service;

        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public ImportArticlesController(ImportArticlesService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public ActionResult PreSettings(int parentId, int id)
        {

            var db = DbService.ReadSettings();
            if (db.RecordActions && db.SingleUserId != QPContext.CurrentUserId)
            {
                throw new Exception(DBStrings.SingeUserModeMessage);
            }
            var content = ContentService.Read(id);
            if (!content.WorkflowBinding.CurrentUserCanUpdateArticles)
            {
                throw new Exception(ArticleStrings.CannotAddBecauseOfWorkflow);
            }

            return Json(_service.MultistepActionSettings(parentId, id));
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public async Task<ActionResult> Settings(string tabId, int parentId, int id)
        {
            return await JsonHtml($"{FolderForTemplate}/ImportTemplate", new ImportViewModel { ContentId = id });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public async Task<ActionResult> FileFields(int parentId, int id, IFormCollection collection)
        {
            var model = new ImportViewModel() { ContentId = id };
            await TryUpdateModelAsync(model);

            model.SetCorrespondingFieldName(collection);
            var settings = model.GetImportSettingsObject(parentId, id);
            return Json(MultistepActionHelper.GetFileFields(settings, new FileReader(settings)));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal) => Json(_service.Setup(parentId, id, boundToExternal));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        [BackendActionLog]
        public async Task<ActionResult> SetupWithParams(int parentId, int id, IFormCollection collection)
        {
            var model = new ImportViewModel() { ContentId = id };
            await TryUpdateModelAsync(model);

            model.SetCorrespondingFieldName(collection);
            IMultistepActionParams settings = model.GetImportSettingsObject(parentId, id);
            _service.SetupWithParams(parentId, id, settings);
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
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return Json(null);
        }
    }
}
