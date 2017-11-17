using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.Audit;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Audit;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionLogController : QPController
    {
        private readonly IBackendActionLogService _actionLogService;
        private readonly IButtonTraceService _buttonTraceService;
        private readonly IRemovedEntitiesService _removedEntitiesService;
        private readonly ISessionLogService _sessionLogService;

        public ActionLogController(IBackendActionLogService actionLogService, IButtonTraceService buttonTraceService, IRemovedEntitiesService removedEntitiesService, ISessionLogService sessionLogService)
        {
            _actionLogService = actionLogService;
            _buttonTraceService = buttonTraceService;
            _removedEntitiesService = removedEntitiesService;
            _sessionLogService = sessionLogService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionLog)]
        [BackendActionContext(ActionCode.ActionLog)]
        public ActionResult Actions(string tabId, int parentId)
        {
            var model = ActionLogAreaViewModel.Create(tabId, parentId);
            model.ActionTypeList = _actionLogService.GetActionTypeList()
                .Select(t => new QPSelectListItem { Text = Translator.Translate(t.NotTranslatedName), Value = t.Code, Selected = false })
                .OrderBy(itm => itm.Text)
                .ToArray();

            model.EntityTypeList = _actionLogService.GetEntityTypeList()
                .Select(t => new QPSelectListItem { Text = Translator.Translate(t.NotTranslatedName), Value = t.Code, Selected = false })
                .OrderBy(itm => itm.Text)
                .ToArray();

            model.ActionList = _actionLogService.GetActionList()
                .Select(t => new QPSelectListItem { Text = Translator.Translate(t.NotTranslatedName), Value = t.Code, Selected = false })
                .OrderBy(itm => itm.Text)
                .ToArray();

            return JsonHtml("Actions", model);
        }

        [ActionAuthorize(ActionCode.ActionLog)]
        [BackendActionContext(ActionCode.ActionLog)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _Actions(GridCommand command, [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<BackendActionLogFilter>))] BackendActionLogFilter filter)
        {
            var list = _actionLogService.GetLogPage(command.GetListCommand(), filter);
            return View(new GridModel
            {
                Data = list.Data.Select(r =>
                {
                    r.ActionTypeName = Translator.Translate(r.ActionTypeName);
                    r.ActionName = Translator.Translate(r.ActionName);
                    r.EntityTypeName = Translator.Translate(r.EntityTypeName);
                    return r;
                }),
                Total = list.TotalRecords
            });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ButtonTrace)]
        [BackendActionContext(ActionCode.ButtonTrace)]
        public ActionResult ButtonTrace(string tabId, int parentId)
        {
            var model = ButtonTraceAreaViewModel.Create(tabId, parentId);
            return JsonHtml("ButtonTraceIndex", model);
        }

        [ActionAuthorize(ActionCode.ButtonTrace)]
        [BackendActionContext(ActionCode.ButtonTrace)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _ButtonTrace(GridCommand command)
        {
            var list = _buttonTraceService.GetPage(command.GetListCommand());
            return View(new GridModel
            {
                Data = list.Data.Select(r =>
                {
                    r.ButtonName = Translator.Translate(r.ButtonName);
                    r.TabName = Translator.Translate(r.TabName);
                    return r;
                }),
                Total = list.TotalRecords
            });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.RemovedEntities)]
        [BackendActionContext(ActionCode.RemovedEntities)]
        public ActionResult RemovedEntities(string tabId, int parentId)
        {
            var model = RemovedEntitiesAreaViewModel.Create(tabId, parentId);
            return JsonHtml("RemovedEntities", model);
        }

        [ActionAuthorize(ActionCode.RemovedEntities)]
        [BackendActionContext(ActionCode.RemovedEntities)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _RemovedEntities(GridCommand command)
        {
            var list = _removedEntitiesService.GetPage(command.GetListCommand());
            return View(new GridModel
            {
                Data = list.Data,
                Total = list.TotalRecords
            });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SuccessfulSession)]
        [BackendActionContext(ActionCode.SuccessfulSession)]
        public ActionResult SucessfullSessions(string tabId, int parentId)
        {
            var model = SucessfullSessionsAreaViewModel.Create(tabId, parentId);
            return JsonHtml("SucessfullSessions", model);
        }

        [ActionAuthorize(ActionCode.SuccessfulSession)]
        [BackendActionContext(ActionCode.SuccessfulSession)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _SucessfullSessions(GridCommand command)
        {
            var list = _sessionLogService.GetSucessfullSessionPage(command.GetListCommand());
            return View(new GridModel
            {
                Data = list.Data,
                Total = list.TotalRecords
            });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.FailedSession)]
        [BackendActionContext(ActionCode.FailedSession)]
        public ActionResult FailedSessions(string tabId, int parentId)
        {
            var model = FailedSessionsAreaViewModel.Create(tabId, parentId);
            return JsonHtml("FailedSessions", model);
        }

        [ActionAuthorize(ActionCode.FailedSession)]
        [BackendActionContext(ActionCode.FailedSession)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _FailedSessions(GridCommand command)
        {
            var list = _sessionLogService.GetFailedSessionPage(command.GetListCommand());
            return View(new GridModel
            {
                Data = list.Data,
                Total = list.TotalRecords
            });
        }
    }
}
