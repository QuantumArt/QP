using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ImportArticlesController : QPController
    {
        private readonly IMultistepActionService _service;

        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public ImportArticlesController(IMultistepActionService service)
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

            return Json(_service.MultistepActionSettings(parentId, id));
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public async Task<ActionResult> Settings(string tabId, int parentId, int id)
        {
            return await JsonHtml($"{FolderForTemplate}/ImportTemplate", new ImportViewModel
            {
                ContentId = id
            });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public async Task<ActionResult> FileFields(int parentId, int id, FormCollection collection)
        {
            var model = new ImportViewModel();
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
        public async Task<ActionResult> SetupWithParams(int parentId, int id, FormCollection collection)
        {
            var model = new ImportViewModel();
            await TryUpdateModelAsync(model);

            model.SetCorrespondingFieldName(collection);
            IMultistepActionParams settings = model.GetImportSettingsObject(parentId, id);
            _service.SetupWithParams(parentId, id, settings);
            return JsonCamelCase(new JSendResponse { Status = JSendStatus.Success });
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step) => Json(_service.Step(stage, step));

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
