using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BackendActionLogAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = BackendActionContextAttribute.FilterOrder + 1;
        private IBackendActionLogRepository _repository;
        private IEnumerable<BackendActionLog> _logs;

        public BackendActionLogAttribute()
        {
            Order = FilterOrder;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _repository = filterContext.HttpContext.RequestServices.GetRequiredService<IBackendActionLogRepository>();
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
