using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System.Diagnostics.Contracts;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    /// <summary>
    /// Режим работы фильтра <see cref="ExceptionResult"/>
    /// </summary>
    public enum ExceptionResultMode
    {
        /// <summary>
        /// Возвращает ошибку в формате для  интерфейсных qp-action
        /// </summary>
        UIAction,
        /// <summary>
        /// Возвращает ошибку в формате для  неинтерфейсных qp-action
        /// </summary>
        OperationAction,
        /// <summary>
        /// Возвращает ошибку в формате JSend
        /// </summary>
        JSendResponse
    }

    /// <summary>
    /// Exception-фильтр: возвращает информацию об ошибку в разных форматах
    /// </summary>
    public class ExceptionResultAttribute : FilterAttribute, IExceptionFilter
    {
        ExceptionResultMode mode;

        private string PolicyName { get; set; }

        public ExceptionResultAttribute(ExceptionResultMode mode) : this(mode, "Policy") { }

        public ExceptionResultAttribute(ExceptionResultMode mode, string policyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(policyName));
            this.mode = mode;
            PolicyName = policyName;
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.Exception == null)
            {
                return;
            }

            var controller = (QPController)filterContext.Controller;
            if (controller == null || controller.IsReplayAction())
            {
                return;
            }

            filterContext.Result = ErrorMessageGenerator.GererateJsonError(mode, filterContext.Exception);
            EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(filterContext.Exception, PolicyName);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
        }
    }
}
