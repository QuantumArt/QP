using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Library;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteFolderController : AuthQpController
    {
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSiteFolder)]
        [BackendActionContext(ActionCode.AddNewSiteFolder)]
        public async Task<ActionResult> New(string tabId, int parentId, int id)
        {
            var folder = SiteFolderService.New(parentId, id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            return await JsonHtml("FolderProperties", model);
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "id")]
        [ActionAuthorize(ActionCode.AddNewSiteFolder)]
        [BackendActionContext(ActionCode.AddNewSiteFolder)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, int id, IFormCollection collection)
        {
            var folder = SiteFolderService.NewForSave(parentId, id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = SiteFolderService.Save(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSiteFolder });
            }

            return await JsonHtml("FolderProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "id")]
        [BackendActionContext(ActionCode.SiteFolderProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var folder = SiteFolderService.Read(id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("FolderProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSiteFolder)]
        [BackendActionContext(ActionCode.UpdateSiteFolder)]
        [BackendActionLog]
        [Record]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var folder = SiteFolderService.ReadForUpdate(id);
            var model = SiteFolderViewModel.Create(folder, tabId, parentId);
            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = SiteFolderService.Update(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return await JsonHtml("FolderProperties", model);
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

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFileProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.SiteFileProperties)]
        public async Task<ActionResult> FileProperties(string tabId, int parentId, string id, string successfulActionCode)
        {
            var file = SiteFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, true);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateSiteFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.UpdateSiteFile)]
        [BackendActionLog]
        public async Task<ActionResult> FileProperties(string tabId, int parentId, string id, IFormCollection collection)
        {
            var file = SiteFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, true);
            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                SiteFolderService.SaveFile(model.File);
                return Redirect("FileProperties", new { tabId, parentId, id = model.Id, successfulActionCode = ActionCode.UpdateSiteFile });
            }

            return await JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveSiteFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
        [BackendActionContext(ActionCode.MultipleRemoveSiteFile)]
        [BackendActionLog]
        public ActionResult MultipleRemoveFiles(int parentId, [FromBody] SelectedFilesViewModel selModel)
        {
            var result = SiteFolderService.RemoveFiles(parentId, selModel.Names);
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
