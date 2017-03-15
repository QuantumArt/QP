using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Library;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteFolderController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSiteFolder)]
        [BackendActionContext(ActionCode.AddNewSiteFolder)]
        public ActionResult New(string tabId, int parentId, int id)
        {
            var folder = SiteFolderService.New(parentId, id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            return JsonHtml("FolderProperties", model);
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "id")]
        [ActionAuthorize(ActionCode.AddNewSiteFolder)]
        [BackendActionContext(ActionCode.AddNewSiteFolder)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
        {
            var folder = SiteFolderService.NewForSave(parentId, id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = SiteFolderService.Save(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSiteFolder });
            }

            return JsonHtml("FolderProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "id")]
        [BackendActionContext(ActionCode.SiteFolderProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var folder = SiteFolderService.Read(id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("FolderProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSiteFolder)]
        [BackendActionContext(ActionCode.UpdateSiteFolder)]
        [BackendActionLog]
        [Record]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var folder = SiteFolderService.ReadForUpdate(id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = SiteFolderService.Update(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return JsonHtml("FolderProperties", model);
        }

        public ActionResult RemovePreAction(int parentId, int id)
        {
            return Json(SiteFolderService.RemovePreAction(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveSiteFolder)]
        [BackendActionContext(ActionCode.RemoveSiteFolder)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id)
        {
            var result = SiteFolderService.Remove(id);
            return JsonMessageResult(result);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFileProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.SiteFileProperties)]
        public ActionResult FileProperties(string tabId, int parentId, string id, string successfulActionCode)
        {
            var file = SiteFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, true);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateSiteFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.UpdateSiteFile)]
        [BackendActionLog]
        public ActionResult FileProperties(string tabId, int parentId, string id, FormCollection collection)
        {
            var file = SiteFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, true);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                SiteFolderService.SaveFile(model.File);
                return Redirect("FileProperties", new { tabId, parentId, id = model.Id, successfulActionCode = ActionCode.UpdateSiteFile });
            }

            return JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveSiteFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.MultipleRemoveSiteFile)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemoveFiles(int parentId, string[] IDs)
        {
            var result = SiteFolderService.RemoveFiles(parentId, IDs);
            return Json(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveSiteFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.RemoveSiteFile)]
        [BackendActionLog]
        public ActionResult RemoveFile(int parentId, string id)
        {
            string[] ids = { id };
            var result = SiteFolderService.RemoveFiles(parentId, ids);
            return Json(result);
        }
    }
}
