using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FieldController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Fields)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = FieldService.InitList(parentId);
            var model = FieldListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Fields)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = FieldService.List(parentId, command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewField)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewField)]
        public ActionResult New(string tabId, int parentId, int? fieldId)
        {
            var field = FieldService.New(parentId, fieldId);
            var model = FieldViewModel.Create(field, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ValidateInput(false)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewField)]
        [BackendActionContext(ActionCode.AddNewField)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, string backendActionCode, FormCollection collection)
        {
            var content = FieldService.New(parentId, null);
            var model = FieldViewModel.Create(content, tabId, parentId);
            var oldLinkId = model.Data.LinkId;
            var oldBackward = model.Data.BackwardField;
            TryUpdateModel(model);
            model.Validate(ModelState);
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
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return JsonHtml("Properties", model);
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

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.FieldProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Field, "id")]
        [BackendActionContext(ActionCode.FieldProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
        {
            var content = FieldService.Read(id);
            var model = FieldViewModel.Create(content, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            model.OrderChanged = orderChanged ?? false;
            model.ViewInListAffected = viewInListAffected ?? false;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateField)]
        [BackendActionContext(ActionCode.UpdateField)]
        [BackendActionLog]
        [Record(ActionCode.FieldProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
        {
            var field = FieldService.ReadForUpdate(id);
            var model = FieldViewModel.Create(field, tabId, parentId);
            var oldOrder = model.Data.Order;
            var oldViewInList = model.Data.ViewInList;
            var oldLinkId = model.Data.LinkId;
            var oldBackward = model.Data.BackwardField;

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = FieldService.Update(model.Data);
                    PersistLinkId(oldLinkId, model.Data.LinkId);
                    PersistBackwardId(oldBackward, model.Data.BackwardField);
                    PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
                }
                catch (UserQueryContentCreateViewException uqe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
                    return JsonHtml("Properties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return JsonHtml("Properties", model);
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

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VirtualFieldProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.VirtualField, "id")]
        [BackendActionContext(ActionCode.VirtualFieldProperties)]
        public ActionResult VirtualProperties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
        {
            var content = FieldService.VirtualRead(id);
            var model = FieldViewModel.Create(content, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            model.OrderChanged = orderChanged ?? false;
            model.ViewInListAffected = viewInListAffected ?? false;
            return JsonHtml("VirtualProperties", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVirtualField)]
        [BackendActionContext(ActionCode.UpdateVirtualField)]
        [BackendActionLog]
        [Record(ActionCode.VirtualFieldProperties)]
        public ActionResult VirtualProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            var field = FieldService.ReadForUpdate(id);
            var model = FieldViewModel.Create(field, tabId, parentId);
            var oldOrder = model.Data.Order;
            var oldViewInList = model.Data.ViewInList;
            TryUpdateModel(model);
            model.Validate(ModelState);
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
                    return JsonHtml("VirtualProperties", model);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return JsonHtml("Properties", model);
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

            return JsonHtml("VirtualProperties", model);
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
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemove(int[] IDs)
        {
            var result = FieldService.MultipleRemove(IDs);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForExport(string tabId, int parentId, int[] IDs)
        {
            var result = FieldService.InitList(parentId);
            var model = new FieldSelectableListViewModel(result, tabId, parentId, IDs, ActionCode.MultipleSelectFieldForExport)
            {
                IsMultiple = true
            };

            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForExport(string tabId, int parentId, string IDs, GridCommand command)
        {
            var serviceResult = FieldService.ListForExport(command.GetListCommand(), parentId, Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForExportExpanded(string tabId, int parentId, int[] IDs)
        {
            var result = FieldService.InitList(parentId);
            var model = new FieldSelectableListViewModel(result, tabId, parentId, IDs, ActionCode.MultipleSelectFieldForExportExpanded)
            {
                IsMultiple = true
            };

            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
        [BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForExportExpanded(string tabId, int parentId, string IDs, GridCommand command)
        {
            var serviceResult = FieldService.ListForExportExpanded(command.GetListCommand(), parentId, Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeField)]
        [BackendActionContext(ActionCode.CreateLikeField)]
        [BackendActionLog]
        public ActionResult Copy(int id, int? forceId, int? forceLinkId, string forceVirtualFieldIds, string forceChildFieldIds, string forceChildLinkIds)
        {
            FieldService.Read(id); // TODO: unused
            var result = FieldService.Copy(
                id,
                forceId,
                forceLinkId,
                forceVirtualFieldIds.ToIntArray(),
                forceChildFieldIds.ToIntArray(),
                forceChildLinkIds.ToIntArray()
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
