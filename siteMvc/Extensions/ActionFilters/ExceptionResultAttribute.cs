using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.BLL.Services.DTO;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Diagnostics.Contracts;

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
        OperationAction
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
            Contract.Requires(!String.IsNullOrEmpty(policyName));            
            this.mode = mode;
            PolicyName = policyName;
        }

        #region IExceptionFilter Members                
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.Exception == null)
                return;
			QPController controller = (QPController)filterContext.Controller;
			if (controller == null || controller.IsReplayAction())
				return;

            // помещаем в результат операции информацию об ошибках в разных форматах в зависимости от режима
            if(mode == ExceptionResultMode.UIAction)
                filterContext.Result = controller.JsonError(filterContext.Exception.Message);
            else if (mode == ExceptionResultMode.OperationAction)
                filterContext.Result = new JsonResult { Data = MessageResult.Error(filterContext.Exception.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

             EnterpriseLibraryContainer.Current
				   .GetInstance<ExceptionManager>()
				   .HandleException(filterContext.Exception, PolicyName); 

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();            
        }

        #endregion
    }
}