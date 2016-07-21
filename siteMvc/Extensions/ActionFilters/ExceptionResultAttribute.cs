using System.Diagnostics.Contracts;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    /// <summary>
    /// Режим работы фильтра
    /// </summary>
    public enum ExceptionResultMode
    {
        /// <summary>
        /// Возвращает ошибку в формате для  интерфейсных qp-action
        /// </summary>
        UiAction,
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
        private readonly ExceptionResultMode _mode;

        private string PolicyName { get; }

        public ExceptionResultAttribute(ExceptionResultMode mode) : this(mode, "Policy") { }

        public ExceptionResultAttribute(ExceptionResultMode mode, string policyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(policyName));
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
            if (controller == null || controller.IsReplayAction())
            {
                return;
            }

            filterContext.Result = ErrorMessageGenerator.GererateJsonError(_mode, filterContext.Exception);
            EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(filterContext.Exception, PolicyName);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
        }
    }
}
