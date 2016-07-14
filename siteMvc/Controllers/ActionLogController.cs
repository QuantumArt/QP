using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.Audit;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Telerik.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL.Services.Audit;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class ActionLogController : QPController
    {
		private readonly IBackendActionLogService actionLogService;
		private readonly IButtonTraceService buttonTraceService;
		private readonly IRemovedEntitiesService removedEntitiesService;
		private readonly ISessionLogService sessionLogService;

		public ActionLogController(	IBackendActionLogService actionLogService, 
									IButtonTraceService buttonTraceService, 
									IRemovedEntitiesService removedEntitiesService,
									ISessionLogService sessionLogService)
		{
			this.actionLogService = actionLogService;
			this.buttonTraceService = buttonTraceService;
			this.removedEntitiesService = removedEntitiesService;
			this.sessionLogService = sessionLogService;
		}

		#region Action Log
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ActionLog)]
		[BackendActionContext(ActionCode.ActionLog)]
		public ActionResult Actions(string tabId, int parentId)
		{
			ActionLogAreaViewModel model = ActionLogAreaViewModel.Create(tabId, parentId);
			model.ActionTypeList = actionLogService.GetActionTypeList()
				.Select(t => new QPSelectListItem { Text = Translator.Translate(t.NotTranslatedName), Value = t.Code, Selected = false })
				.OrderBy(itm => itm.Text)
				.ToArray();
			model.EntityTypeList = actionLogService.GetEntityTypeList()
				.Select(t => new QPSelectListItem { Text = Translator.Translate(t.NotTranslatedName), Value = t.Code, Selected = false })
				.OrderBy(itm => itm.Text)
				.ToArray();
			return this.JsonHtml("Actions", model);
		}

		[ActionAuthorize(ActionCode.ActionLog)]
		[BackendActionContext(ActionCode.ActionLog)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _Actions(GridCommand command,
			[Bind(Prefix="searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<BackendActionLogFilter>))]BackendActionLogFilter filter
		)
		{
			ListResult<BackendActionLog> list = actionLogService.GetLogPage(command.GetListCommand(), filter);
			return View(new GridModel()
			{
				Data = list.Data.Select(r =>
					{
						r.ActionTypeName = Translator.Translate(r.ActionTypeName);
						r.EntityTypeName = Translator.Translate(r.EntityTypeName);
						return r;
					}),
				Total = list.TotalRecords
			});
		} 
		#endregion

		#region Button Trace
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ButtonTrace)]
		[BackendActionContext(ActionCode.ButtonTrace)]
		public ActionResult ButtonTrace(string tabId, int parentId)
		{
			ButtonTraceAreaViewModel model = ButtonTraceAreaViewModel.Create(tabId, parentId);
			return this.JsonHtml("ButtonTraceIndex", model);
		}

		[ActionAuthorize(ActionCode.ButtonTrace)]
		[BackendActionContext(ActionCode.ButtonTrace)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _ButtonTrace(GridCommand command)
		{
			ListResult<ButtonTrace> list = buttonTraceService.GetPage(command.GetListCommand());
			return View(new GridModel()
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
		#endregion

		#region Removed Entities
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.RemovedEntities)]
		[BackendActionContext(ActionCode.RemovedEntities)]
		public ActionResult RemovedEntities(string tabId, int parentId)
		{
			RemovedEntitiesAreaViewModel model = RemovedEntitiesAreaViewModel.Create(tabId, parentId);
			return this.JsonHtml("RemovedEntities", model);
		}

		[ActionAuthorize(ActionCode.RemovedEntities)]
		[BackendActionContext(ActionCode.RemovedEntities)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _RemovedEntities(GridCommand command)
		{
			ListResult<RemovedEntity> list = removedEntitiesService.GetPage(command.GetListCommand());
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}
		#endregion

		#region Sessions Log 
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SuccessfulSession)]
		[BackendActionContext(ActionCode.SuccessfulSession)]
		public ActionResult SucessfullSessions(string tabId, int parentId)
		{
			SucessfullSessionsAreaViewModel model = SucessfullSessionsAreaViewModel.Create(tabId, parentId);
			return this.JsonHtml("SucessfullSessions", model);
		}

		[ActionAuthorize(ActionCode.SuccessfulSession)]
		[BackendActionContext(ActionCode.SuccessfulSession)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _SucessfullSessions(GridCommand command)
		{
			ListResult<SessionsLog> list = sessionLogService.GetSucessfullSessionPage(command.GetListCommand());
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}


		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.FailedSession)]
		[BackendActionContext(ActionCode.FailedSession)]
		public ActionResult FailedSessions(string tabId, int parentId)
		{
			FailedSessionsAreaViewModel model = FailedSessionsAreaViewModel.Create(tabId, parentId);
			return this.JsonHtml("FailedSessions", model);
		}

		[ActionAuthorize(ActionCode.FailedSession)]
		[BackendActionContext(ActionCode.FailedSession)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _FailedSessions(GridCommand command)
		{
			ListResult<SessionsLog> list = sessionLogService.GetFailedSessionPage(command.GetListCommand());
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}
		#endregion

    }
}
