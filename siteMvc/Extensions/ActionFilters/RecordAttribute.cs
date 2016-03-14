using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System.Globalization;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class RecordAttribute : ActionFilterAttribute, IActionFilter
	{
		public static readonly int FilterOrder = BackendActionLogAttribute.FilterOrder + 1;

		private string code = null;
		private bool ignoreForm = false;

		public RecordAttribute()
		{
			Order = FilterOrder;
		}

		public RecordAttribute(string actionCode) : this()
		{
			code = actionCode;
		}

		public RecordAttribute(string actionCode, bool ignoreForm) : this(actionCode)
		{
			this.ignoreForm = ignoreForm;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var db = DbService.ReadSettings();
			if (db.RecordActions && db.SingleUserId != QPContext.CurrentUserId)
			{
				throw new Exception(DBStrings.SingeUserModeMessage);
			}

			base.OnActionExecuting(filterContext);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.Exception == null 
				&& filterContext.Controller.ViewData.ModelState.IsValid 
				&& DbRepository.Get().RecordActions
				&& !QPController.IsError(filterContext.HttpContext)
			)
			{
				RecordedAction action = new RecordedAction();
				NameValueCollection form = filterContext.HttpContext.Request.Form;
				if (form != null && !ignoreForm)
				{
					action.Form = form;
				}

				action.Code = (code != null) ? code : BackendActionContext.Current.ActionCode;
				action.ParentId = (BackendActionContext.Current.ParentEntityId.HasValue) ? BackendActionContext.Current.ParentEntityId.Value : 0;
				action.Lcid = CultureInfo.CurrentCulture.LCID;
				action.Executed = DateTime.Now;
				action.ExecutedBy = (filterContext.HttpContext.User.Identity as QPIdentity).Name;
				int fromId = filterContext.HttpContext.Items.Contains("FROM_ID") ? (int)filterContext.HttpContext.Items["FROM_ID"] : 0;
				action.Ids = (fromId != 0) ? new string[] { fromId.ToString() } : BackendActionContext.Current.Entities.Select(n => n.StringId).ToArray();
				IRecordHelper helper = DependencyResolver.Current.GetService<IRecordHelper>();
				helper.PersistAction(action, filterContext.HttpContext);
			}

			base.OnActionExecuted(filterContext);
		}
	}
}