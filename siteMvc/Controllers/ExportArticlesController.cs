using System;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ExportArticlesController : QPController
    {
        private readonly IMultistepActionService service;
        private readonly string folderForTemplate = "MultistepSettingsTemplates";

        public ExportArticlesController(IMultistepActionService service)
        {
            this.service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult PreSettings(int parentId, int id)
        {
            IMultistepActionSettings prms = service.MultistepActionSettings(parentId, id, null);
            return Json(prms);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult Settings(string tabId, int parentId, int id)
        {
            ExportViewModel model = new ExportViewModel();
            model.ContentId = id;
            string viewName = String.Format("{0}/ExportTemplate", folderForTemplate);
            return this.JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
        public ActionResult SetupWithParams(int parentId, int id, FormCollection collection)
        {
            ExportViewModel model = new ExportViewModel();
            TryUpdateModel(model);
            ExportSettings settings = new ExportSettings(parentId, null)
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
				settings.FieldIdsToExpand = model.FieldsToExpand.ToArray();
			}

            service.SetupWithParams(parentId, id, (IMultistepActionParams)settings);
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
