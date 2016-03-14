using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{	
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class BackendActionLogAttribute : ActionFilterAttribute, IActionFilter
	{
		public static readonly int FilterOrder = BackendActionContextAttribute.FilterOrder + 1;

		private readonly IBackendActionLogRepository repository = null;
		IEnumerable<BackendActionLog> logs = null;

		public BackendActionLogAttribute()
		{
			Order = FilterOrder;
			repository = DependencyResolver.Current.GetService<IBackendActionLogRepository>();
		}


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			logs = BackendActionLog.CreateLogs(BackendActionContext.Current, repository);
			base.OnActionExecuting(filterContext);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.Exception == null)
			{
				if (BackendActionContext.Current.IsChanged)
					logs = BackendActionLog.CreateLogs(BackendActionContext.Current, repository);
				logs = repository.Save(logs);
			}
			base.OnActionExecuted(filterContext);
		}
	}
}