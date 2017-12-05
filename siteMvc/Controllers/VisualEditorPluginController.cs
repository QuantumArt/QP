using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;
using Telerik.Web.Mvc;

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
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _visualEditorService.InitList(parentId);
            var model = VisualEditorListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.VisualEditorPlugins)]
        [BackendActionContext(ActionCode.VisualEditorPlugins)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _visualEditorService.GetVisualEditorPlugins(command.GetListCommand(), parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VisualEditorPluginProperties)]
        [BackendActionContext(ActionCode.VisualEditorPluginProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var plugin = _visualEditorService.ReadVisualEditorPluginProperties(id);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.VisualEditorPluginProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVisualEditorPlugin)]
        [BackendActionContext(ActionCode.UpdateVisualEditorPlugin)]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var plugin = _visualEditorService.ReadVisualEditorPluginPropertiesForUpdate(id);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                var oldIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
                model.Data = _visualEditorService.UpdateVisualEditorProperties(model.Data);
                var newIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
                PersistResultId(model.Data.Id);
                PersistCommandIds(oldIds, newIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateVisualEditorPlugin });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveVisualEditorPlugin)]
        [BackendActionContext(ActionCode.RemoveVisualEditorPlugin)]
        [BackendActionLog]
        public ActionResult Remove(int id) => JsonMessageResult(_visualEditorService.Remove(id));

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
        public ActionResult New(string tabId, int parentId)
        {
            var plugin = _visualEditorService.NewVisualEditorPluginProperties(parentId);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var plugin = _visualEditorService.NewVisualEditorPluginPropertiesForUpdate(parentId);
            var model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _visualEditorService.SaveVisualEditorPluginProperties(model.Data);
                PersistResultId(model.Data.Id);
                PersistCommandIds(null, model.Data.VeCommands.Select(n => n.Id).ToArray());
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveVisualEditorPlugin });
            }

            return JsonHtml("Properties", model);
        }
    }
}
