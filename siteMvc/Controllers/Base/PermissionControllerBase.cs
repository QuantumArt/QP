using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
    public abstract class PermissionControllerBase : AuthQpController
    {
        protected readonly IPermissionService Service;

        protected PermissionControllerBase(IPermissionService service)
        {
            Service = service;
        }

        protected abstract string ControllerName { get; }

        public virtual async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = Service.InitList(parentId);
            var model = PermissionListViewModel.Create(result, tabId, parentId, Service, ControllerName);
            return await JsonHtml("EntityPermissionIndex", model);
        }

        public virtual ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = Service.List(parentId, listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        public virtual async Task<ActionResult> New(string tabId, int parentId)
        {
            var permission = Service.New(parentId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);
            return await JsonHtml("EntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> New(string tabId, int parentId, FormCollection collection)
        {
            var permission = Service.New(parentId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);

            await TryUpdateModelAsync(model);

            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = Service.Save(model.Data);
                    PersistResultId(model.Data.Id);
                    return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSitePermission });
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return await JsonHtml("EntityPermissionProperties", model);
                }
            }

            return await JsonHtml("EntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var permission = Service.Read(id);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("EntityPermissionProperties", model);
        }

        public virtual async Task<ActionResult> Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var permission = Service.ReadForUpdate(id);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);

            await TryUpdateModelAsync(model);

            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = Service.Update(model.Data);
                    return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSitePermission });
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return await JsonHtml("EntityPermissionProperties", model);
                }
            }

            return await JsonHtml("EntityPermissionProperties", model);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public virtual ActionResult MultipleRemove(int parentId, int[] IDs) => JsonMessageResult(Service.MultipleRemove(parentId, IDs));

        public virtual ActionResult Remove(int parentId, int id) => JsonMessageResult(Service.Remove(parentId, id));
    }
}
