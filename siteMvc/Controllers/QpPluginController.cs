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
using Quantumart.QP8.WebMvc.ViewModels.QpPlugin;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class QpPluginController : AuthQpController
    {
        private readonly IQpPluginService _pluginService;

        public QpPluginController(IQpPluginService pluginService)
        {
            _pluginService = pluginService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.QpPlugins)]
        [BackendActionContext(ActionCode.QpPlugins)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = _pluginService.InitList(parentId);
            var model = QpPluginListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.QpPlugins)]
        [BackendActionContext(ActionCode.QpPlugins)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _pluginService.List(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.QpPluginProperties)]
        [BackendActionContext(ActionCode.QpPluginProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var plugin = _pluginService.Read(id);
            var model = QpPluginViewModel.Create(plugin, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.QpPluginProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateQpPlugin)]
        [BackendActionContext(ActionCode.UpdateQpPlugin)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var plugin = _pluginService.ReadForUpdate(id);
            var model = QpPluginViewModel.Create(plugin, tabId, parentId);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = _pluginService.Update(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateQpPlugin });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveQpPlugin)]
        [BackendActionContext(ActionCode.RemoveQpPlugin)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            return JsonMessageResult(_pluginService.Remove(id));
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewQpPlugin)]
        [BackendActionContext(ActionCode.AddNewQpPlugin)]
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var plugin = _pluginService.New(parentId);
            var model = QpPluginViewModel.Create(plugin, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewQpPlugin)]
        [BackendActionContext(ActionCode.AddNewQpPlugin)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var plugin = _pluginService.NewForSave(parentId);
            var model = QpPluginViewModel.Create(plugin, tabId, parentId);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = _pluginService.Save(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveQpPlugin });
            }

            return await JsonHtml("Properties", model);
        }
    }

}
