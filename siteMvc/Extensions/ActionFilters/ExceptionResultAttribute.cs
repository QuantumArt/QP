using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    public class ExceptionResultAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly ExceptionResultMode _mode;

        private string PolicyName { get; }

        public ExceptionResultAttribute(ExceptionResultMode mode)
            : this(mode, "Policy")
        {
        }

        public ExceptionResultAttribute(ExceptionResultMode mode, string policyName)
        {
            Ensure.Argument.NotNullOrWhiteSpace(policyName, nameof(policyName));

            _mode = mode;
            PolicyName = policyName;
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext?.Exception == null)
            {
                return;
            }

            var controller = (QPController)filterContext.Controller;
            if (controller == null || CommonHelpers.IsXmlDbUpdateReplayAction(filterContext.HttpContext))
            {
                return;
            }

            filterContext.Result = ActionResultHelpers.GererateJsonError(_mode, filterContext.Exception);
            EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(filterContext.Exception, PolicyName);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
        }
    }
}
