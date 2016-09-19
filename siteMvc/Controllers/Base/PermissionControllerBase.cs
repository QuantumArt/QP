using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
    public abstract class PermissionControllerBase : QPController
    {
        protected readonly IPermissionService Service;

        protected PermissionControllerBase(IPermissionService service)
        {
            Service = service;
        }

        protected abstract string ControllerName { get; }

        public virtual ActionResult Index(string tabId, int parentId)
        {
            var result = Service.InitList(parentId);
            var model = PermissionListViewModel.Create(result, tabId, parentId, Service, ControllerName);
            return JsonHtml("EntityPermissionIndex", model);
        }

        public virtual ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = Service.List(parentId, command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        public virtual ActionResult New(string tabId, int parentId)
        {
            var permission = Service.New(parentId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);
            return JsonHtml("EntityPermissionProperties", model);
        }

        public virtual ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var permission = Service.New(parentId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);

            TryUpdateModel(model);
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
                    return JsonHtml("EntityPermissionProperties", model);
                }
            }

            return JsonHtml("EntityPermissionProperties", model);
        }

        public virtual ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var permission = Service.Read(id);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("EntityPermissionProperties", model);
        }

        public virtual ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var permission = Service.ReadForUpdate(id);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service);

            TryUpdateModel(model);
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
                    return JsonHtml("EntityPermissionProperties", model);
                }
            }

            return JsonHtml("EntityPermissionProperties", model);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public virtual ActionResult MultipleRemove(int parentId, int[] IDs)
        {
            var result = Service.MultipleRemove(parentId, IDs);
            return JsonMessageResult(result);
        }

        public virtual ActionResult Remove(int parentId, int id)
        {
            var result = Service.Remove(parentId, id);
            return JsonMessageResult(result);
        }
    }
}
