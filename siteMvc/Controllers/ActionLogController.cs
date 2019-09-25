using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.Audit;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Audit;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionLogController : AuthQpController
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

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionLog)]
        [BackendActionContext(ActionCode.ActionLog)]
        public async Task<ActionResult> Actions(string tabId, int parentId)
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

            return await JsonHtml("Actions", model);
        }

        [ActionAuthorize(ActionCode.ActionLog)]
        [BackendActionContext(ActionCode.ActionLog)]
        public ActionResult _Actions(
            int page,
            int pageSize,
            string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<BackendActionLogFilter>))]
            BackendActionLogFilter filter)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var list = _actionLogService.GetLogPage(listCommand, filter);

            var data = list.Data.Select(log =>
            {
                log.ActionTypeName = Translator.Translate(log.ActionTypeName);
                log.ActionName = Translator.Translate(log.ActionName);
                log.EntityTypeName = Translator.Translate(log.EntityTypeName);
                return log;
            });

            return new TelerikResult(data, list.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ButtonTrace)]
        [BackendActionContext(ActionCode.ButtonTrace)]
        public async Task<ActionResult> ButtonTrace(string tabId, int parentId)
        {
            return await JsonHtml("ButtonTraceIndex", ButtonTraceAreaViewModel.Create(tabId, parentId));
        }

        [ActionAuthorize(ActionCode.ButtonTrace)]
        [BackendActionContext(ActionCode.ButtonTrace)]
        public ActionResult _ButtonTrace(int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var list = _buttonTraceService.GetPage(listCommand);

            var data = list.Data.Select(trace =>
            {
                trace.ButtonName = Translator.Translate(trace.ButtonName);
                trace.TabName = Translator.Translate(trace.TabName);
                return trace;
            });

            return new TelerikResult(data, list.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.RemovedEntities)]
        [BackendActionContext(ActionCode.RemovedEntities)]
        public async Task<ActionResult> RemovedEntities(string tabId, int parentId)
        {
            var model = RemovedEntitiesAreaViewModel.Create(tabId, parentId);
            return await JsonHtml("RemovedEntities", model);
        }

        [ActionAuthorize(ActionCode.RemovedEntities)]
        [BackendActionContext(ActionCode.RemovedEntities)]
        public ActionResult _RemovedEntities(int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var list = _removedEntitiesService.GetPage(listCommand);
            return new TelerikResult(list.Data, list.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SuccessfulSession)]
        [BackendActionContext(ActionCode.SuccessfulSession)]
        public async Task<ActionResult> SucessfullSessions(string tabId, int parentId)
        {
            var model = SucessfullSessionsAreaViewModel.Create(tabId, parentId);
            return await JsonHtml("SucessfullSessions", model);
        }

        [ActionAuthorize(ActionCode.SuccessfulSession)]
        [BackendActionContext(ActionCode.SuccessfulSession)]
        public ActionResult _SucessfullSessions(int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var list = _sessionLogService.GetSucessfullSessionPage(listCommand);
            return new TelerikResult(list.Data, list.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.FailedSession)]
        [BackendActionContext(ActionCode.FailedSession)]
        public async Task<ActionResult> FailedSessions(string tabId, int parentId)
        {
            var model = FailedSessionsAreaViewModel.Create(tabId, parentId);
            return await JsonHtml("FailedSessions", model);
        }

        [ActionAuthorize(ActionCode.FailedSession)]
        [BackendActionContext(ActionCode.FailedSession)]
        public ActionResult _FailedSessions(int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var list = _sessionLogService.GetFailedSessionPage(listCommand);
            return new TelerikResult(list.Data, list.TotalRecords);
        }
    }
}
