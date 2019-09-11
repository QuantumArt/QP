using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.CopySite;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.MultistepSettings;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CopySiteController : QPController
    {
        private readonly IMultistepActionService _multistepService;
        private const string FolderForTemplate = "MultistepSettingsTemplates";

        public CopySiteController(CopySiteService multistepService)
        {
            _multistepService = multistepService ?? throw new ArgumentNullException(nameof(multistepService));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        public ActionResult PreSettings(int parentId, int id)
        {
            var prms = _multistepService.MultistepActionSettings(parentId, id);
            return Json(prms);
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        public async Task<ActionResult> Settings(string tabId, int parentId, int id)
        {
            var model = ViewModel.Create<CreateLikeSiteModel>(tabId, parentId);
            model.Data = SiteService.Read(id);
            var viewName = $"{FolderForTemplate}/CreateLikeSiteTemplate";
            return await JsonHtml(viewName, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        [BackendActionLog]
        public ActionResult Setup(int parentId, int id, bool? boundToExternal)
        {
            var settings = _multistepService.Setup(parentId, id, boundToExternal);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CreateLikeSite)]
        [BackendActionContext(ActionCode.CreateLikeSite)]
        [ConnectionScope]
        [BackendActionLog]
        [Record]
        public async Task<ActionResult> SetupWithParams(string tabId, int parentId, int id, IFormCollection collection)
        {
            var newSite = SiteService.NewForSave();
            var model = CreateLikeSiteModel.Create(newSite, tabId, parentId);

            await TryUpdateModelAsync(model);

            var sourceSite = SiteService.Read(id);
            model.Data.AssemblingType = sourceSite.AssemblingType;
            TryValidateModel(model);

            var viewName = $"{FolderForTemplate}/CreateLikeSiteTemplate";
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = SiteService.Save(model.Data, new List<int>().ToArray(), new List<int>().ToArray());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Exception", ex.Message);
                    return await JsonCamelCaseHtml(viewName, model);
                }

                var settings = new CopySiteSettings(newSite.Id, id, DateTime.Now, model.DoNotCopyArticles, model.DoNotCopyTemplates, model.DoNotCopyFiles);
                _multistepService.SetupWithParams(parentId, id, settings);
                return Json(new JSendResponse { Status = JSendStatus.Success });
            }

            return await JsonCamelCaseHtml(viewName, model);
        }

        [HttpPost]
        [NoTransactionConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult Step(int stage, int step)
        {
            var stepResult = _multistepService.Step(stage, step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _multistepService.TearDown();
            return null;
        }
    }
}
