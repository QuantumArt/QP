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
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
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
        public ActionResult _Actions(
            int page,
            int pageSize,
            string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<BackendActionLogFilter>))]
            BackendActionLogFilter filter)
        {
            var list = _actionLogService.GetLogPage(GetListCommand(page, pageSize, orderBy), filter);

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
        public ActionResult ButtonTrace(string tabId, int parentId) => JsonHtml("ButtonTraceIndex", ButtonTraceAreaViewModel.Create(tabId, parentId));

        [ActionAuthorize(ActionCode.ButtonTrace)]
        [BackendActionContext(ActionCode.ButtonTrace)]
        public ActionResult _ButtonTrace(int page, int pageSize, string orderBy)
        {
            var list = _buttonTraceService.GetPage(GetListCommand(page, pageSize, orderBy));

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
        public ActionResult RemovedEntities(string tabId, int parentId)
        {
            var model = RemovedEntitiesAreaViewModel.Create(tabId, parentId);
            return JsonHtml("RemovedEntities", model);
        }

        [ActionAuthorize(ActionCode.RemovedEntities)]
        [BackendActionContext(ActionCode.RemovedEntities)]
        public ActionResult _RemovedEntities(int page, int pageSize, string orderBy)
        {
            var list = _removedEntitiesService.GetPage(GetListCommand(page, pageSize, orderBy));
            return new TelerikResult(list.Data, list.TotalRecords);
        }

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
        public ActionResult _SucessfullSessions(int page, int pageSize, string orderBy)
        {
            var list = _sessionLogService.GetSucessfullSessionPage(GetListCommand(page, pageSize, orderBy));
            return new TelerikResult(list.Data, list.TotalRecords);
        }

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
        public ActionResult _FailedSessions(int page, int pageSize, string orderBy)
        {
            var list = _sessionLogService.GetFailedSessionPage(GetListCommand(page, pageSize, orderBy));
            return new TelerikResult(list.Data, list.TotalRecords);
        }

        private static ListCommand GetListCommand(int page, int pageSize, string orderBy)
        {
            return new ListCommand
            {
                StartPage = page,
                PageSize = pageSize,
                SortExpression = GridExtensions.ToSqlSortExpression(orderBy ?? "")
            };
        }
    }
}
