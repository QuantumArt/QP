using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FormatController : AuthQpController
    {
        private readonly IFormatService _formatService;

        public FormatController(IFormatService formatService)
        {
            _formatService = formatService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjectFormats)]
        [BackendActionContext(ActionCode.PageObjectFormats)]
        public async Task<ActionResult> IndexPageObjectFormats(string tabId, int parentId)
        {
            var result = _formatService.InitFormatList(parentId, false);
            var model = ObjectFormatListViewModel.Create(result, tabId, parentId, false);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.PageObjectFormats)]
        [BackendActionContext(ActionCode.PageObjectFormats)]
        public ActionResult _IndexPageObjectFormats(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _formatService.GetPageObjectFormatsByObjectId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjectFormats)]
        [BackendActionContext(ActionCode.TemplateObjectFormats)]
        public async Task<ActionResult> IndexTemplateObjectFormats(string tabId, int parentId)
        {
            var result = _formatService.InitFormatList(parentId, true);
            var model = ObjectFormatListViewModel.Create(result, tabId, parentId, true);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.TemplateObjectFormats)]
        [BackendActionContext(ActionCode.TemplateObjectFormats)]
        public ActionResult _IndexTemplateObjectFormats(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _formatService.GetTemplateObjectFormatsByObjectId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjectFormatVersions)]
        [BackendActionContext(ActionCode.PageObjectFormatVersions)]
        public async Task<ActionResult> IndexPageObjectFormatVersions(string tabId, int parentId)
        {
            var result = _formatService.InitFormatVersionList();
            var model = ObjectFormatVersionListViewModel.Create(result, tabId, parentId, false);
            return await JsonHtml("IndexVersions", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.PageObjectFormatVersions)]
        [BackendActionContext(ActionCode.PageObjectFormatVersions)]
        public ActionResult _IndexPageObjectFormatVersions(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _formatService.GetPageObjectFormatVersionsByFormatId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjectFormatVersions)]
        [BackendActionContext(ActionCode.TemplateObjectFormatVersions)]
        public async Task<ActionResult> IndexTemplateObjectFormatVersions(string tabId, int parentId)
        {
            var result = _formatService.InitFormatVersionList();
            var model = ObjectFormatVersionListViewModel.Create(result, tabId, parentId, true);
            return await JsonHtml("IndexVersions", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.TemplateObjectFormatVersions)]
        [BackendActionContext(ActionCode.TemplateObjectFormatVersions)]
        public ActionResult _IndexTemplateObjectFormatVersions(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _formatService.GetTemplateObjectFormatVersionsByFormatId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelTemplateObjectFormat)]
        [BackendActionContext(ActionCode.CancelTemplateObjectFormat)]
        public ActionResult CancelTemplateObjectFormat(int id)
        {
            _formatService.CancelFormat(id, false);
            return Json(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelPageObjectFormat)]
        [BackendActionContext(ActionCode.CancelPageObjectFormat)]
        public ActionResult CancelPageObjectFormat(int id)
        {
            _formatService.CancelFormat(id, true);
            return Json(null);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewPageObjectFormat)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.AddNewPageObjectFormat)]
        public async Task<ActionResult> NewPageObjectFormat(string tabId, int parentId)
        {
            var format = _formatService.NewPageObjectFormatProperties(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewPageObjectFormat)]
        [BackendActionContext(ActionCode.AddNewPageObjectFormat)]
        [BackendActionLog]
        public async Task<ActionResult> NewPageObjectFormat(string tabId, int parentId, IFormCollection collection)
        {
            var format = _formatService.NewPageObjectFormatPropertiesForUpdate(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _formatService.SaveObjectFormatProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("PageObjectFormatProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SavePageObjectFormat });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewTemplateObjectFormat)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.TemplateObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.AddNewTemplateObjectFormat)]
        public async Task<ActionResult> NewTemplateObjectFormat(string tabId, int parentId)
        {
            var format = _formatService.NewTemplateObjectFormatProperties(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewTemplateObjectFormat)]
        [BackendActionContext(ActionCode.AddNewTemplateObjectFormat)]
        [BackendActionLog]
        public async Task<ActionResult> NewTemplateObjectFormat(string tabId, int parentId, IFormCollection collection)
        {
            var format = _formatService.NewTemplateObjectFormatPropertiesForUpdate(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = _formatService.SaveObjectFormatProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("TemplateObjectFormatProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveTemplateObjectFormat });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjectFormatProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "id")]
        [BackendActionContext(ActionCode.PageObjectFormatProperties)]
        public async Task<ActionResult> PageObjectFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var format = _formatService.ReadPageObjectFormatProperties(id, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
            ViewData[SpecialKeys.IsEntityReadOnly] = format.LockedByAnyoneElse;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdatePageObjectFormat)]
        [BackendActionContext(ActionCode.UpdatePageObjectFormat)]
        [BackendActionLog]
        [Record(ActionCode.PageObjectFormatProperties)]
        public async Task<ActionResult> PageObjectFormatProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var format = _formatService.ReadPageObjectFormatPropertiesForUpdate(id, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _formatService.UpdateObjectFormatProperties(model.Data);
                return Redirect("PageObjectFormatProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdatePageObjectFormat });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjectFormatProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "id")]
        [BackendActionContext(ActionCode.TemplateObjectFormatProperties)]
        public async Task<ActionResult> TemplateObjectFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var format = _formatService.ReadTemplateObjectFormatProperties(id, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
            ViewData[SpecialKeys.IsEntityReadOnly] = format.LockedByAnyoneElse;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateTemplateObjectFormat)]
        [BackendActionContext(ActionCode.UpdateTemplateObjectFormat)]
        [BackendActionLog]
        [Record(ActionCode.TemplateObjectFormatProperties)]
        public async Task<ActionResult> TemplateObjectFormatProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var format = _formatService.ReadTemplateObjectFormatPropertiesForUpdate(id, _formatService.IsSiteDotNetByObjectId(parentId));
            var model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _formatService.UpdateObjectFormatProperties(model.Data);
                return Redirect("TemplateObjectFormatProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateTemplateObjectFormat });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjectFormatVersionProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormatVersion, "id")]
        [BackendActionContext(ActionCode.TemplateObjectFormatVersionProperties)]
        public async Task<ActionResult> TemplateObjectFormatVersionProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var version = _formatService.ReadTemplateObjectFormatVersionProperties(id);
            var model = ObjectFormatVersionViewModel.Create(version, tabId, parentId, false);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("VersionProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjectFormatVersionProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormatVersion, "parentId")]
        [BackendActionContext(ActionCode.PageObjectFormatVersionProperties)]
        public async Task<ActionResult> PageObjectFormatVersionProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var version = _formatService.ReadPageObjectFormatVersionProperties(id);
            var model = ObjectFormatVersionViewModel.Create(version, tabId, parentId, true);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("VersionProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareWithCurrentTemplateObjectFormatVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.CompareWithCurrentTemplateObjectFormatVersion)]
        public async Task<ActionResult> CompareWithCurrentTemplateObjectFormatVersion(string tabId, int parentId, int id, string successfulActionCode)
        {
            var version = _formatService.GetMergedObjectFormatVersion(new[] { id, ObjectFormatVersion.CurrentVersionId }, parentId, false);
            var model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, false);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("CompareObjectFormatVersionProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareWithCurrentPageObjectFormatVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.CompareWithCurrentPageObjectFormatVersion)]
        public async Task<ActionResult> CompareWithCurrentPageObjectFormatVersion(string tabId, int parentId, int id, string successfulActionCode)
        {
            var version = _formatService.GetMergedObjectFormatVersion(new[] { id, ObjectFormatVersion.CurrentVersionId }, parentId, true);
            var model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, true);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("CompareObjectFormatVersionProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareTemplateObjectFormatVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.CompareTemplateObjectFormatVersions)]
        public async Task<ActionResult> CompareTemplateObjectFormatVersions(
            string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel
        )
        {
            var version = _formatService.GetMergedObjectFormatVersion(selModel.Ids, parentId, false);
            var model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, false);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            return await JsonHtml("CompareObjectFormatVersionProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ComparePageObjectFormatVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "parentId")]
        [BackendActionContext(ActionCode.ComparePageObjectFormatVersions)]
        public async Task<ActionResult> ComparePageObjectFormatVersions(
            string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel
        )
        {
            var version = _formatService.GetMergedObjectFormatVersion(selModel.Ids, parentId, true);
            var model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, true);
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            ViewData[SpecialKeys.IsEntityReadOnly] = true;
            return await JsonHtml("CompareObjectFormatVersionProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveTemplateObjectFormat)]
        [BackendActionContext(ActionCode.RemoveTemplateObjectFormat)]
        [BackendActionLog]
        [Record]
        public ActionResult RemoveTemplateObjectFormat(int id)
        {
            var result = _formatService.RemoveObjectFormat(id, false);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemovePageObjectFormat)]
        [BackendActionContext(ActionCode.RemovePageObjectFormat)]
        [BackendActionLog]
        [Record]
        public ActionResult RemovePageObjectFormat(int id)
        {
            var result = _formatService.RemoveObjectFormat(id, true);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveTemplateObjectFormatVersion)]
        [BackendActionContext(ActionCode.MultipleRemoveTemplateObjectFormatVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult MultipleRemoveTemplateObjectFormatVersion(int parentId,[FromBody] SelectedItemsViewModel selModel)
        {
            return JsonMessageResult(_formatService.MultipleRemoveObjectFormatVersion(selModel.Ids));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.RestorePageObjectFormatVersion)]
        [BackendActionContext(ActionCode.RestorePageObjectFormatVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult RestorePageObjectFormatVersion(string tabId, int parentId, int id) => JsonMessageResult(_formatService.RestoreObjectFormatVersion(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.RestoreTemplateObjectFormatVersion)]
        [BackendActionContext(ActionCode.RestoreTemplateObjectFormatVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult RestoreTemplateObjectFormatVersion(string tabId, int parentId, int id) => JsonMessageResult(_formatService.RestoreObjectFormatVersion(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemovePageObjectFormatVersion)]
        [BackendActionContext(ActionCode.MultipleRemovePageObjectFormatVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult MultipleRemovePageObjectFormatVersion(int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            return JsonMessageResult(_formatService.MultipleRemoveObjectFormatVersion(selModel.Ids));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockTemplateObjectFormat)]
        [BackendActionContext(ActionCode.CaptureLockTemplateObjectFormat)]
        [BackendActionLog]
        public ActionResult CaptureLockTemplateObjectFormat(int id)
        {
            _formatService.CaptureLockTemplateObjectFormat(id);
            return JsonMessageResult(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockPageObjectFormat)]
        [BackendActionContext(ActionCode.CaptureLockPageObjectFormat)]
        [BackendActionLog]
        public ActionResult CaptureLockPageObjectFormat(int id)
        {
            _formatService.CaptureLockPageObjectFormat(id);
            return Json(null);
        }
    }
}
