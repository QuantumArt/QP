using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VisualEditorPluginController : QPController
    {
        private readonly IVisualEditorService _visualEditorService;

        public VisualEditorPluginController(IVisualEditorService visualEditorService)
        {
            _visualEditorService = visualEditorService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VisualEditorPlugins)]
        [BackendActionContext(ActionCode.VisualEditorPlugins)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = _visualEditorService.InitList(parentId);
            var model = VisualEditorListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.VisualEditorPlugins)]
        [BackendActionContext(ActionCode.VisualEditorPlugins)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _visualEditorService.GetVisualEditorPlugins(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VisualEditorPluginProperties)]
        [BackendActionContext(ActionCode.VisualEditorPluginProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var plugin = _visualEditorService.ReadVisualEditorPluginProperties(id);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.VisualEditorPluginProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVisualEditorPlugin)]
        [BackendActionContext(ActionCode.UpdateVisualEditorPlugin)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var plugin = _visualEditorService.ReadVisualEditorPluginPropertiesForUpdate(id);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);

            await TryUpdateModelAsync(model);
            model.DoCustomBinding();
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                var oldIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
                model.Data = _visualEditorService.UpdateVisualEditorProperties(model.Data);
                var newIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
                PersistResultId(model.Data.Id);
                PersistCommandIds(oldIds, newIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateVisualEditorPlugin });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveVisualEditorPlugin)]
        [BackendActionContext(ActionCode.RemoveVisualEditorPlugin)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            return JsonMessageResult(_visualEditorService.Remove(id));
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var plugin = _visualEditorService.NewVisualEditorPluginProperties(parentId);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var plugin = _visualEditorService.NewVisualEditorPluginPropertiesForUpdate(parentId);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);

            await TryUpdateModelAsync(model);
            model.DoCustomBinding();
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                model.Data = _visualEditorService.SaveVisualEditorPluginProperties(model.Data);
                PersistResultId(model.Data.Id);
                PersistCommandIds(null, model.Data.VeCommands.Select(n => n.Id).ToArray());
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveVisualEditorPlugin });
            }

            return await JsonHtml("Properties", model);
        }
    }
}
