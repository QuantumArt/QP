using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using System;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
    public abstract class PermissionWithChildControllerBase : PermissionControllerBase
    {
        protected readonly IChildEntityPermissionService childContentService;

        public PermissionWithChildControllerBase(IPermissionService service, IChildEntityPermissionService childContentService)
            : base(service)
        {
            this.childContentService = childContentService;
        }

        #region list
        public virtual ActionResult ChildIndex(string tabId, int parentId)
        {
            ChildPermissionInitListResult result = childContentService.InitList(parentId);
            ChildEntityPermissionListViewModel model = ChildEntityPermissionListViewModel.Create(result, tabId, parentId, childContentService.ListViewModelSettings, ControllerName);
            return this.JsonHtml("ChildEntityPermissionIndex", model);
        }

        public virtual ActionResult _ChildIndex(string tabId, int parentId, int? userId, int? groupId, GridCommand command)
        {
            ListResult<ChildEntityPermissionListItem> serviceResult = childContentService.List(parentId, groupId, userId, command.GetListCommand());
            return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }
        #endregion

        #region Changes
        #region MultipleChange
        public virtual ActionResult MultipleChangeAsChild(string tabId, int parentId, int[] IDs, int? userId, int? groupId)
        {
            ChildEntityPermission permission = ChildEntityPermission.Create(childContentService, parentId, userId, groupId);
            ChildEntityPermissionViewModel model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, MultipleChangeAction, ControllerName, "SaveMultipleChangeAsChild", childContentService, userId, groupId, IDs);
            return this.JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual ActionResult SaveMultipleChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, collection, model => { childContentService.MultipleChange(parentId, model.EntityIDs, model.Data); });
        }
        #endregion

        #region AllChange
        public virtual ActionResult AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return new JsonNetResult<object>(new { success = false, message = EntityPermissionStrings.UserOrGroupAreNotSelected });
            }
            else
            {
                var permission = ChildEntityPermission.Create(childContentService, parentId, userId, groupId);
                var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, AllChangeAction, ControllerName, "AllChangeAsChild", childContentService, userId, groupId);
                return JsonHtml("ChildEntityPermissionProperties", model);
            }
        }

        public virtual ActionResult AllChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, collection, model => { childContentService.ChangeAll(parentId, model.Data); });
        }
        #endregion

        #region Change
        public virtual ActionResult ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
        {
            ChildEntityPermission permission = childContentService.Read(parentId, id, userId, groupId) ?? ChildEntityPermission.Create(childContentService, parentId, userId, groupId);
            ChildEntityPermissionViewModel model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, ChangeAction, ControllerName, "ChangeAsChild", childContentService, userId, groupId, new[] { id });
            return this.JsonHtml("ChildEntityPermissionProperties", model);
        }


        public virtual ActionResult ChangeAsChild(string tabId, int parentId, FormCollection collection)
        {
            return SaveAsChild(tabId, parentId, collection, model => { childContentService.Change(parentId, model.EntityIDs.FirstOrDefault(), model.Data); });
        }
        #endregion

        #region common save
        protected abstract string SaveChildPermissionAction { get; }
        protected abstract string MultipleChangeAction { get; }
        protected abstract string AllChangeAction { get; }
        protected abstract string ChangeAction { get; }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        public ActionResult SaveAsChild(string tabId, int parentId)
        {
            if (TempData.ContainsKey(GetChildEntityPermissionModelKey(tabId, parentId)))
            {
                ChildEntityPermissionViewModel model = TempData[GetChildEntityPermissionModelKey(tabId, parentId)] as ChildEntityPermissionViewModel;
                return this.JsonHtml("ChildEntityPermissionProperties", model);
            }
            else
                throw new ApplicationException("TempData is empty.");
        }

        private ActionResult SaveAsChild(string tabId, int parentId, FormCollection collection, Action<ChildEntityPermissionViewModel> action)
        {
            try
            {
                ChildEntityPermission permission = ChildEntityPermission.Create(childContentService, parentId);
                ChildEntityPermissionViewModel model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, SaveChildPermissionAction, ControllerName, null, childContentService, isPostBack: true);
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
                return Redirect("SaveAsChild", new { tabId = tabId, parentId = parentId });
            }
            catch
            {
                TempData.Remove(GetChildEntityPermissionModelKey(tabId, parentId));
                throw;
            }
        }

        private string GetChildEntityPermissionModelKey(string tabId, int parentId)
        {
            return String.Format("ChildEntityPermissionViewModel_{0}_{1}", tabId, parentId);
        }
        #endregion
        #endregion

        #region Remove
        public virtual ActionResult MultipleRemoveAsChild(int parentId, int[] IDs, int? userId, int? groupId)
        {
            return JsonMessageResult(childContentService.MultipleRemove(parentId, IDs, userId, groupId));
        }

        public virtual ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
                return JsonMessageResult(MessageResult.Error(EntityPermissionStrings.UserOrGroupAreNotSelected));
            else
                return JsonMessageResult(childContentService.RemoveAll(parentId, userId, groupId));
        }

        public virtual ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
        {
            return JsonMessageResult(childContentService.Remove(parentId, id, userId, groupId));
        }
        #endregion
    }
}
