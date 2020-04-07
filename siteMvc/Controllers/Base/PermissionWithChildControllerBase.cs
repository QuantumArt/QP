using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

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

        public virtual async Task<ActionResult> ChildIndex(string tabId, int parentId)
        {
            var result = ChildContentService.InitList(parentId);
            var model = ChildEntityPermissionListViewModel.Create(result, tabId, parentId, ChildContentService.ListViewModelSettings, ControllerName);
            return await JsonHtml("ChildEntityPermissionIndex", model);
        }

        public virtual ActionResult _ChildIndex(
            string tabId,
            int parentId,
            int? userId,
            int? groupId,
            int page,
            int pageSize,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ChildContentService.List(parentId, groupId, userId, listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        public virtual async Task<ActionResult> MultipleChangeAsChild(string tabId, int parentId, [FromBody] SelectedItemsViewModel idsModel, int? userId, int? groupId)
        {
            var permission = ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, MultipleChangeAction, ControllerName, "SaveMultipleChangeAsChild", ChildContentService, userId, groupId, idsModel.Ids);
            return await JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> SaveMultipleChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await SaveAsChild(tabId, parentId, model => { ChildContentService.MultipleChange(parentId, model.EntityIds, model.Data); });
        }

        public virtual async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return Json(new { success = false, message = EntityPermissionStrings.UserOrGroupAreNotSelected });
            }

            var permission = ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, AllChangeAction, ControllerName, "AllChangeAsChild", ChildContentService, userId, groupId);
            return await JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await SaveAsChild(tabId, parentId, model => { ChildContentService.ChangeAll(parentId, model.Data); });
        }

        public virtual async Task<ActionResult> ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
        {
            var permission = ChildContentService.Read(parentId, id, userId, groupId) ?? ChildEntityPermission.Create(ChildContentService, parentId, userId, groupId);
            var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, ChangeAction, ControllerName, "ChangeAsChild", ChildContentService, userId, groupId, new[] { id });
            return await JsonHtml("ChildEntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> ChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await SaveAsChild(tabId, parentId, model => { ChildContentService.Change(parentId, model.EntityIds.FirstOrDefault(), model.Data); });
        }

        protected abstract string SaveChildPermissionAction { get; }

        protected abstract string MultipleChangeAction { get; }

        protected abstract string AllChangeAction { get; }

        protected abstract string ChangeAction { get; }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> SaveAsChild(string tabId, int parentId)
        {
            if (TempData.ContainsKey(GetChildEntityPermissionModelKey(tabId, parentId)))
            {
                var model = JsonConvert.DeserializeObject<ChildEntityPermissionViewModel>(
                    TempData[GetChildEntityPermissionModelKey(tabId, parentId)].ToString()
                );
                return await JsonHtml("ChildEntityPermissionProperties", model);
            }

            throw new ApplicationException("TempData is empty.");
        }

        private async Task<ActionResult> SaveAsChild(string tabId, int parentId, Action<ChildEntityPermissionViewModel> action)
        {
            try
            {
                var permission = ChildEntityPermission.Create(ChildContentService, parentId);
                var model = ChildEntityPermissionViewModel.Create(permission, tabId, parentId, SaveChildPermissionAction, ControllerName, null, ChildContentService, isPostBack: true);

                await TryUpdateModelAsync(model);

                try
                {
                    action(model);
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return await JsonHtml("ChildEntityPermissionProperties", model);
                }

                TempData[GetChildEntityPermissionModelKey(tabId, parentId)] = JsonConvert.SerializeObject(model);
                return Redirect("SaveAsChild", new { tabId, parentId });
            }
            catch
            {
                TempData.Remove(GetChildEntityPermissionModelKey(tabId, parentId));
                throw;
            }
        }

        private static string GetChildEntityPermissionModelKey(string tabId, int parentId) => $"ChildEntityPermissionViewModel_{tabId}_{parentId}";

        public virtual ActionResult MultipleRemoveAsChild(int parentId, [FromBody] SelectedItemsViewModel model, int? userId, int? groupId)
        {
            return JsonMessageResult(ChildContentService.MultipleRemove(parentId, model.Ids, userId, groupId));
        }

        public virtual ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return JsonMessageResult(MessageResult.Error(EntityPermissionStrings.UserOrGroupAreNotSelected));
            }

            return JsonMessageResult(ChildContentService.RemoveAll(parentId, userId, groupId));
        }

        public virtual ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId) => JsonMessageResult(ChildContentService.Remove(parentId, id, userId, groupId));
    }
}
