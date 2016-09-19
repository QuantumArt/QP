using System.Web.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VirtualContentController : QPController
    {
        [HttpPost]
        public JsonResult GetChildFieldList(int virtualContentId, int? joinedContentId, string entityId, string selectItemIDs, string parentAlias)
        {
            var entityList = VirtualContentService.GetChildFieldList(virtualContentId, joinedContentId, entityId, selectItemIDs, parentAlias);
            return Json(entityList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVirtualContents)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewVirtualContents)]
        public ActionResult New(string tabId, int parentId, int? groupId)
        {
            var content = VirtualContentService.New(parentId, groupId);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVirtualContents)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewVirtualContents)]
        [BackendActionLog]
        [Record]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var content = VirtualContentService.NewForSave(parentId);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = VirtualContentService.Save(model.Data);
                    PersistResultId(model.Data.Id);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                    return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveVirtualContent });
                }
                catch (UserQueryContentCreateViewException uqe)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return JsonHtml("Properties", model);
                }
                catch (CycleInContentGraphException)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);
                    return JsonHtml("Properties", model);
                }
            }

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.VirtualContentProperties)]
        [BackendActionContext(ActionCode.VirtualContentProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged)
        {
            var content = VirtualContentService.Read(id);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            model.GroupChanged = groupChanged ?? false;
            model.SuccesfulActionCode = successfulActionCode;

            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVirtualContent)]
        [BackendActionContext(ActionCode.UpdateVirtualContent)]
        [BackendActionLog]
        [Record(ActionCode.VirtualContentProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
        {
            var content = VirtualContentService.ReadForUpdate(id);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            var oldGroupId = model.Data.GroupId;
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = VirtualContentService.Update(model.Data);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                }
                catch (UserQueryContentCreateViewException uqe)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return JsonHtml("Properties", model);
                }
                catch (CycleInContentGraphException)
                {
                    if (IsReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);
                    return JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveVirtualContent)]
        [BackendActionContext(ActionCode.RemoveVirtualContent)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id)
        {
            return JsonMessageResult(VirtualContentService.Remove(id));
        }
    }
}
