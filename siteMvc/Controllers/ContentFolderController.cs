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
    public class ContentFolderController : AuthQpController
    {
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewContentFolder)]
        [BackendActionContext(ActionCode.AddNewContentFolder)]
        public async Task<ActionResult> New(string tabId, int parentId, int id)
        {
            var folder = ContentFolderService.New(parentId, id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);
            return await JsonHtml("FolderProperties", model);
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewContentFolder)]
        [BackendActionContext(ActionCode.AddNewContentFolder)]
        [BackendActionLog]
        [Record]
        public async Task<ActionResult> New(string tabId, int parentId, int id, IFormCollection collection)
        {
            var folder = ContentFolderService.NewForSave(parentId, id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = ContentFolderService.Save(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSiteFolder });
            }

            return await JsonHtml("FolderProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentFolderProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "id")]
        [BackendActionContext(ActionCode.ContentFolderProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var folder = ContentFolderService.Read(id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("FolderProperties", model);
        }

        [HttpPost, Record(ActionCode.ContentFolderProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContentFolder)]
        [BackendActionContext(ActionCode.UpdateContentFolder)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var folder = ContentFolderService.ReadForUpdate(id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = ContentFolderService.Update(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return await JsonHtml("FolderProperties", model);
        }

        public ActionResult RemovePreAction(int parentId, int id) => Json(ContentFolderService.RemovePreAction(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveContentFolder)]
        [BackendActionContext(ActionCode.RemoveContentFolder)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id)
        {
            var result = ContentFolderService.Remove(id);
            return JsonMessageResult(result);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentFileProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.ContentFileProperties)]
        public async Task<ActionResult> FileProperties(string tabId, int parentId, string id, string successfulActionCode)
        {
            var file = ContentFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, false);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.UpdateContentFile)]
        [BackendActionLog]
        public async Task<ActionResult> FileProperties(string tabId, int parentId, string id, IFormCollection collection)
        {
            var file = ContentFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, false);
            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                ContentFolderService.SaveFile(model.File);
                return Redirect("FileProperties", new { tabId, parentId, id = model.Id, successfulActionCode = ActionCode.UpdateContentFile });
            }

            return await JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.MultipleRemoveContentFile)]
        [BackendActionLog]
        public ActionResult MultipleRemoveFiles(int parentId, [FromBody] SelectedFilesViewModel model)
        {
            var result = ContentFolderService.RemoveFiles(parentId, model.Names);
            return Json(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.RemoveContentFile)]
        [BackendActionLog]
        public ActionResult RemoveFile(int parentId, string id)
        {
            string[] ids = { id };
            var result = ContentFolderService.RemoveFiles(parentId, ids);
            return Json(result);
        }
    }
}
