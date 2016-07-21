using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BackendActionLogAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = BackendActionContextAttribute.FilterOrder + 1;
        private readonly IBackendActionLogRepository _repository;
        private IEnumerable<BackendActionLog> _logs;

        public BackendActionLogAttribute()
        {
            Order = FilterOrder;
            _repository = DependencyResolver.Current.GetService<IBackendActionLogRepository>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _logs = BackendActionLog.CreateLogs(BackendActionContext.Current, _repository);
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null)
            {
                if (BackendActionContext.Current.IsChanged)
                {
                    _logs = BackendActionLog.CreateLogs(BackendActionContext.Current, _repository);
                }

                _logs = _repository.Save(_logs);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
