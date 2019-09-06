using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VirtualContentController : QPController
    {
        [HttpPost]
        public JsonResult GetChildFieldList(int virtualContentId, int? joinedContentId, string entityId, string selectItemIDs, string parentAlias)
        {
            var entityList = VirtualContentService.GetChildFieldList(virtualContentId, joinedContentId, entityId, selectItemIDs, parentAlias);
            return Json(entityList);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVirtualContents)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewVirtualContents)]
        public async Task<ActionResult> New(string tabId, int parentId, int? groupId)
        {
            var content = VirtualContentService.New(parentId, groupId);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVirtualContents)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewVirtualContents)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var content = VirtualContentService.NewForSave(parentId);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            await TryUpdateModelAsync(model);
            model.DoCustomBinding();
            TryValidateModel(model);
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
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return await JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return await JsonHtml("Properties", model);
                }
                catch (CycleInContentGraphException)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);
                    return await JsonHtml("Properties", model);
                }
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.VirtualContentProperties)]
        [BackendActionContext(ActionCode.VirtualContentProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged)
        {
            var content = VirtualContentService.Read(id);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            model.GroupChanged = groupChanged ?? false;
            model.SuccesfulActionCode = successfulActionCode;

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.VirtualContentProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVirtualContent)]
        [BackendActionContext(ActionCode.UpdateVirtualContent)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string backendActionCode, IFormCollection collection)
        {
            var content = VirtualContentService.ReadForUpdate(id);
            var model = VirtualContentViewModel.Create(content, tabId, parentId);
            var oldGroupId = model.Data.GroupId;
            await TryUpdateModelAsync(model);
            model.DoCustomBinding();
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = VirtualContentService.Update(model.Data);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                }
                catch (UserQueryContentCreateViewException uqe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return await JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return await JsonHtml("Properties", model);
                }
                catch (CycleInContentGraphException)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);
                    return await JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveVirtualContent)]
        [BackendActionContext(ActionCode.RemoveVirtualContent)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            return JsonMessageResult(VirtualContentService.Remove(id));
        }
    }
}
