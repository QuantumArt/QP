using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Field;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FieldController : AuthQpController
    {
        public FieldController(IArticleService dbArticleService, QPublishingOptions options)
            : base(dbArticleService, options)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Fields)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = FieldService.InitList(parentId);
            var model = FieldListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Fields)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = FieldService.List(parentId, listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewField)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewField)]
        public async Task<ActionResult> New(string tabId, int parentId, int? fieldId)
        {
            var field = FieldService.New(parentId, fieldId);
            var model = FieldViewModel.Create(field, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewField)]
        [BackendActionContext(ActionCode.AddNewField)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, string backendActionCode, IFormCollection collection)
        {
            var content = FieldService.New(parentId, null);
            var model = FieldViewModel.Create(content, tabId, parentId);
            var oldLinkId = model.Data.LinkId;
            var oldBackward = model.Data.BackwardField;

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = FieldService.Save(model.Data);
                    PersistResultId(model.Data.Id);
                    PersistLinkId(oldLinkId, model.Data.LinkId);
                    PersistBackwardId(oldBackward, model.Data.BackwardField);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                    PersistChildFieldIds(model.Data.ResultChildFieldIds);
                    PersistChildLinkIds(model.Data.ResultChildLinkIds);
                    AppendFormGuidsFromIds("DefaultArticleIds", "DefaultArticleUniqueIds");
                    AppendFormGuidsFromIds("Data.O2MDefaultValue", "Data.O2MUniqueIdDefaultValue");
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

                return Redirect("Properties", new
                {
                    tabId,
                    parentId,
                    id = model.Data.Id,
                    successfulActionCode = backendActionCode,
                    viewInListAffected = model.Data.ViewInList
                });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.FieldProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Field, "id")]
        [BackendActionContext(ActionCode.FieldProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
        {
            var content = FieldService.Read(id);
            var model = FieldViewModel.Create(content, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            model.OrderChanged = orderChanged ?? false;
            model.ViewInListAffected = viewInListAffected ?? false;
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.FieldProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateField)]
        [BackendActionContext(ActionCode.UpdateField)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string backendActionCode, IFormCollection collection)
        {
            var field = FieldService.ReadForUpdate(id);
            var model = FieldViewModel.Create(field, tabId, parentId);
            var oldOrder = model.Data.Order;
            var oldViewInList = model.Data.ViewInList;
            var oldLinkId = model.Data.LinkId;
            var oldBackward = model.Data.BackwardField;

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = FieldService.Update(model.Data);
                    PersistLinkId(oldLinkId, model.Data.LinkId);
                    PersistBackwardId(oldBackward, model.Data.BackwardField);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                    AppendFormGuidsFromIds("DefaultArticleIds", "DefaultArticleUniqueIds");
                    AppendFormGuidsFromIds("Data.O2MDefaultValue", "Data.O2MUniqueIdDefaultValue");
                }
                catch (UserQueryContentCreateViewException uqEx)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqEx.Message);
                    return await JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpEx)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpEx.Message);
                    return await JsonHtml("Properties", model);
                }

                var newOrder = model.Data.Order;
                var newViewInList = model.Data.ViewInList;
                return Redirect("Properties", new
                {
                    tabId,
                    parentId,
                    id = model.Data.Id,
                    successfulActionCode = backendActionCode,
                    orderChanged = newOrder != oldOrder,
                    viewInListAffected = newViewInList != oldViewInList
                });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VirtualFieldProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.VirtualField, "id")]
        [BackendActionContext(ActionCode.VirtualFieldProperties)]
        public async Task<ActionResult> VirtualProperties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
        {
            var content = FieldService.VirtualRead(id);
            var model = FieldViewModel.Create(content, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            model.OrderChanged = orderChanged ?? false;
            model.ViewInListAffected = viewInListAffected ?? false;
            return await JsonHtml("VirtualProperties", model);
        }

        [HttpPost, Record(ActionCode.VirtualFieldProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVirtualField)]
        [BackendActionContext(ActionCode.UpdateVirtualField)]
        [BackendActionLog]
        public async Task<ActionResult> VirtualProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var field = FieldService.ReadForUpdate(id);
            var model = FieldViewModel.Create(field, tabId, parentId);
            var oldOrder = model.Data.Order;
            var oldViewInList = model.Data.ViewInList;

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = VirtualFieldService.Update(model.Data);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                }
                catch (UserQueryContentCreateViewException uqe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return await JsonHtml("VirtualProperties", model);
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

                var newViewInList = model.Data.ViewInList;
                return Redirect("VirtualProperties", new
                {
                    tabId,
                    parentId,
                    id = model.Data.Id,
                    successfulActionCode = ActionCode.UpdateField,
                    orderChanged = oldOrder != model.Data.Order,
                    viewInListAffected = newViewInList != oldViewInList
                });
            }

            return await JsonHtml("VirtualProperties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveField)]
        [BackendActionContext(ActionCode.RemoveField)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            var result = FieldService.Remove(id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveField)]
        [BackendActionContext(ActionCode.MultipleRemoveField)]
        [BackendActionLog]
        public ActionResult MultipleRemove([FromBody] SelectedItemsViewModel selModel)
        {
            var result = FieldService.MultipleRemove(selModel.Ids);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
        public async Task<ActionResult> MultipleSelectForExport(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = FieldService.InitList(parentId);
            var model = new FieldSelectableListViewModel(result, tabId, parentId, selModel.Ids, ActionCode.MultipleSelectFieldForExport)
            {
                IsMultiple = true
            };

            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
        public ActionResult _MultipleSelectForExport(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = FieldService.ListForExport(listCommand, parentId, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
        public async Task<ActionResult> MultipleSelectForExportExpanded(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = FieldService.InitList(parentId);
            var model = new FieldSelectableListViewModel(result, tabId, parentId, selModel.Ids, ActionCode.MultipleSelectFieldForExportExpanded)
            {
                IsMultiple = true
            };

            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
        public ActionResult _MultipleSelectForExportExpanded(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = FieldService.ListForExportExpanded(
                listCommand, parentId, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeField)]
        [BackendActionContext(ActionCode.CreateLikeField)]
        [BackendActionLog]
        public ActionResult Copy(int id, int? forceId, int? forceLinkId, string forceVirtualFieldIds, string forceChildFieldIds, string forceChildLinkIds)
        {
            var result = FieldService.Copy(
                id,
                forceId,
                forceLinkId,
                forceVirtualFieldIds?.ToIntArray(),
                forceChildFieldIds?.ToIntArray(),
                forceChildLinkIds?.ToIntArray()
            );

            PersistResultId(result.Id);
            PersistFromId(id);
            PersistLinkId(null, result.LinkId);
            PersistVirtualFieldIds(result.VirtualFieldIds);
            PersistChildFieldIds(result.ChildFieldIds);
            PersistChildLinkIds(result.ChildLinkIds);
            return JsonMessageResult(result.Message);
        }
    }
}
