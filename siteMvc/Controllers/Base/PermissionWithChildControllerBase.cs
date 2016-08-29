using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
    public abstract class PermissionWithChildControllerBase : PermissionControllerBase
    {
        protected readonly IChildEntityPermissionService ChildContentService;

        protected PermissionWithChildControllerBase(IPermissionService service, IChildEntityPermissionService childContentService)
            : base(service)
        {
            ChildContentService = childContentService;
        }

        public virtual ActionResult ChildIndex(string tabId, int parentId)
        {
            var result = ChildContentService.InitList(parentId);
            var model = ChildEntityPermissionListViewModel.Create(result, tabId, parentId, ChildContentService.ListViewModelSettings, ControllerName);
            return JsonHtml("ChildEntityPermissionIndex", model);
        }

        public virtual ActionResult _ChildIndex(string tabId, int parentId, int? userId, int? groupId, GridCommand command)
        {
            var serviceResult = ChildContentService.List(parentId, groupId, userId, command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public virtual ActionResult MultipleChangeAsChild(string tabId, int parentId, int[] IDs, int? userId, int? groupId)
        {
            var permission = ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, MultipleChangeAction, ControllerName, "SaveMultipleChangeAsChild", ChildContentService, userId, groupId, IDs);
            return JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual ActionResult SaveMultipleChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, model => { ChildContentService.MultipleChange(parentId, model.EntityIDs, model.Data); });
        }

        public virtual ActionResult AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return new JsonNetResult<object>(new { success = false, message = EntityPermissionStrings.UserOrGroupAreNotSelected });
            }

            var permission = ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, AllChangeAction, ControllerName, "AllChangeAsChild", ChildContentService, userId, groupId);
            return JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual ActionResult AllChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, model => { ChildContentService.ChangeAll(parentId, model.Data); });
        }

        public virtual ActionResult ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
        {
            var permission = ChildContentService.Read(parentId, id, userId, groupId) ?? ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, ChangeAction, ControllerName, "ChangeAsChild", ChildContentService, userId, groupId, new[] { id });
            return JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual ActionResult ChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, model => { ChildContentService.Change(parentId, model.EntityIDs.FirstOrDefault(), model.Data); });
        }

        protected abstract string SaveChildPermissionAction { get; }

        protected abstract string MultipleChangeAction { get; }

        protected abstract string AllChangeAction { get; }

        protected abstract string ChangeAction { get; }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult SaveAsChild(string tabId, int parentId)
        {
            if (TempData.ContainsKey(GetChildEntityPermissionModelKey(tabId, parentId)))
            {
                var model = TempData[GetChildEntityPermissionModelKey(tabId, parentId)] as ChildEntityPermissionViewModel;
                return JsonHtml("ChildEntityPermissionProperties", model);
            }

            throw new ApplicationException("TempData is empty.");
        }

        private ActionResult SaveAsChild(string tabId, int parentId, Action<ChildEntityPermissionViewModel> action)
        {
            try
            {
                var permission = ChildEntityPermission.Create(ChildContentService, parentId);
                var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, SaveChildPermissionAction, ControllerName, null, ChildContentService, isPostBack: true);
                TryUpdateModel(model);
                try
                {
                    action(model);
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return JsonHtml("ChildEntityPermissionProperties", model);
                }
                TempData[GetChildEntityPermissionModelKey(tabId, parentId)] = model;
                return Redirect("SaveAsChild", new { tabId, parentId });
            }
            catch
            {
                TempData.Remove(GetChildEntityPermissionModelKey(tabId, parentId));
                throw;
            }
        }

        private static string GetChildEntityPermissionModelKey(string tabId, int parentId)
        {
            return $"ChildEntityPermissionViewModel_{tabId}_{parentId}";
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public virtual ActionResult MultipleRemoveAsChild(int parentId, int[] IDs, int? userId, int? groupId)
        {
            return JsonMessageResult(ChildContentService.MultipleRemove(parentId, IDs, userId, groupId));
        }

        public virtual ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return JsonMessageResult(MessageResult.Error(EntityPermissionStrings.UserOrGroupAreNotSelected));
            }

            return JsonMessageResult(ChildContentService.RemoveAll(parentId, userId, groupId));
        }

        public virtual ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
        {
            return JsonMessageResult(ChildContentService.Remove(parentId, id, userId, groupId));
        }
    }
}
