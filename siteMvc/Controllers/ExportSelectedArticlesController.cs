using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ExportSelectedArticlesController : QPController
    {
        private readonly IMultistepActionService service;
        private readonly string folderForTemplate = "MultistepSettingsTemplates";

        public ExportSelectedArticlesController(IMultistepActionService service)
        {
            this.service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult PreSettings(int parentId, int[] IDs)
        {
            IMultistepActionSettings prms = service.MultistepActionSettings(parentId, 0, IDs);
            return Json(prms);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        public ActionResult Settings(string tabId, int parentId, int[] IDs)
        {
            ExportViewModel model = new ExportViewModel();
            model.ContentId = parentId;
            model.Ids = IDs;
            string viewName = String.Format("{0}/ExportTemplate", folderForTemplate);
            return this.JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
		public ActionResult Setup(int parentId, int[] IDs, bool? boundToExternal)
        {
            MultistepActionSettings settings = service.Setup(parentId, 0, IDs, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ExportArticles)]
        [BackendActionContext(ActionCode.ExportArticles)]
        [BackendActionLog]
        public ActionResult SetupWithParams(int parentId, int[] IDs, FormCollection collection)
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
			}
			settings.FieldIdsToExpand = model.FieldsToExpand.ToArray();

            service.SetupWithParams(parentId, IDs, (IMultistepActionParams)settings);
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
