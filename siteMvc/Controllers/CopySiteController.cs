using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.CopySite;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CopySiteController : QPController
    {
        private readonly IMultistepActionService multistepService;
        private readonly string folderForTemplate = "MultistepSettingsTemplates";

        public CopySiteController(IMultistepActionService multistepService)
        {
            if (multistepService == null)
                throw new ArgumentNullException("multistepService");
            this.multistepService = multistepService;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        public ActionResult PreSettings(int parentId, int id)
        {
            IMultistepActionSettings prms = multistepService.MultistepActionSettings(parentId, id);
            return Json(prms);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        public ActionResult Settings(string tabId, int parentId, int id)
        {
            CreateLikeSiteModel model = EntityViewModel.Create<CreateLikeSiteModel>(tabId, parentId);
            model.Data = SiteService.Read(id);
            string viewName = String.Format("{0}/CreateLikeSiteTemplate", folderForTemplate);
            return this.JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        [BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            MultistepActionSettings settings = multistepService.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [BackendActionLog]
        [ValidateInput(false)]
        [Record]
        public ActionResult SetupWithParams(string tabId, int parentId, int id, FormCollection collection)
        {
            Site newSite = SiteService.NewForSave();
            CreateLikeSiteModel model = CreateLikeSiteModel.Create(newSite, tabId, parentId);
            TryUpdateModel(model);
            Site sourceSite = SiteService.Read(id);
            model.Data.AssemblingType = sourceSite.AssemblingType;
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                List<int> emptyArray = new List<int>();
                model.Data = SiteService.Save(model.Data, emptyArray.ToArray(), emptyArray.ToArray());
                this.PersistResultId(model.Data.Id);

                IMultistepActionParams settings = new CopySiteSettings(newSite.Id, id, DateTime.Now, model.DoNotCopyArticles, model.DoNotCopyTemplates, model.DoNotCopyFiles);

                multistepService.SetupWithParams(parentId, id, settings);

                return Json(String.Empty);
            }
            else
            {
                string viewName = String.Format("{0}/CreateLikeSiteTemplate", folderForTemplate);
                return JsonHtml(viewName, model);
            }
        }

        [HttpPost]
        [NoTransactionConnectionScopeAttribute]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            MultistepActionStepResult stepResult = multistepService.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            multistepService.TearDown();
            return null;
        }


    }
}
