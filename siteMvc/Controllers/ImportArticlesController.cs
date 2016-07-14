using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ImportArticlesController : QPController
    {
        private readonly IMultistepActionService service;
        private readonly string folderForTemplate = "MultistepSettingsTemplates";

        public ImportArticlesController(IMultistepActionService service)
        {
            this.service = service;
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

			IMultistepActionSettings prms = service.MultistepActionSettings(parentId, id);
            return Json(prms);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public ActionResult Settings(string tabId, int parentId, int id, int blockedFieldId)
        {
            ImportViewModel model = new ImportViewModel();
            model.ContentId = id;
			model.BlockedFieldId = blockedFieldId;
            string viewName = String.Format("{0}/ImportTemplate", folderForTemplate);
            return this.JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        public ActionResult FileFields(int parentId, int id, FormCollection collection)
        {
            ImportViewModel model = new ImportViewModel();
            TryUpdateModel(model);
            model.SetCorrespondingFieldName(collection);
            ImportSettings settings = model.GetImportSettingsObject(parentId, id);
			FileReader reader = new FileReader(settings);
            List<string> fieldsList = MultistepActionHelper.GetFileFields(settings, reader);
            return Json(fieldsList);
        }
        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        [BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ImportArticles)]
        [BackendActionContext(ActionCode.ImportArticles)]
        [BackendActionLog]
        public ActionResult SetupWithParams(int parentId, int id, FormCollection collection)
        {
            ImportViewModel model = new ImportViewModel();
            TryUpdateModel(model);
            model.SetCorrespondingFieldName(collection);
            IMultistepActionParams settings = model.GetImportSettingsObject(parentId, id);
            service.SetupWithParams(parentId, id, settings);
            return Json(String.Empty);
        }

        [HttpPost]
        [NoTransactionConnectionScopeAttribute]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            MultistepActionStepResult stepResult = service.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            service.TearDown();
            return null;
        }

    }
}
