using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Library;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ContentFolderController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewContentFolder)]
        [BackendActionContext(ActionCode.AddNewContentFolder)]
        public ActionResult New(string tabId, int parentId, int id)
        {
            var folder = ContentFolderService.New(parentId, id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);
            return JsonHtml("FolderProperties", model);
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewContentFolder)]
        [BackendActionContext(ActionCode.AddNewContentFolder)]
        [BackendActionLog]
        [Record]
        public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
        {
            var folder = ContentFolderService.NewForSave(parentId, id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = ContentFolderService.Save(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSiteFolder });
            }

            return JsonHtml("FolderProperties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentFolderProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "id")]
        [BackendActionContext(ActionCode.ContentFolderProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var folder = ContentFolderService.Read(id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("FolderProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContentFolder)]
        [BackendActionContext(ActionCode.UpdateContentFolder)]
        [BackendActionLog]
        [Record(ActionCode.ContentFolderProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var folder = ContentFolderService.ReadForUpdate(id);
            var model = ContentFolderViewModel.Create(folder, tabId, parentId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = ContentFolderService.Update(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return JsonHtml("FolderProperties", model);
        }

        public ActionResult RemovePreAction(int parentId, int id)
        {
            return Json(ContentFolderService.RemovePreAction(id));
        }

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

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentFileProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.ContentFileProperties)]
        public ActionResult FileProperties(string tabId, int parentId, string id, string successfulActionCode)
        {
            var file = ContentFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, false);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.UpdateContentFile)]
        [BackendActionLog]
        public ActionResult FileProperties(string tabId, int parentId, string id, FormCollection collection)
        {
            var file = ContentFolderService.GetFile(parentId, id);
            var model = FileViewModel.Create(file, tabId, parentId, false);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                ContentFolderService.SaveFile(model.File);
                return Redirect("FileProperties", new { tabId, parentId, id = model.Id, successfulActionCode = ActionCode.UpdateContentFile });
            }

            return JsonHtml("FileProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.MultipleRemoveContentFile)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemoveFiles(int parentId, string[] IDs)
        {
            var result = ContentFolderService.RemoveFiles(parentId, IDs);
            return Json(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveContentFile)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
        [BackendActionContext(ActionCode.RemoveContentFile)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult RemoveFile(int parentId, string id)
        {
            string[] IDs = { id };
            var result = ContentFolderService.RemoveFiles(parentId, IDs);
            return Json(result);
        }
    }
}
