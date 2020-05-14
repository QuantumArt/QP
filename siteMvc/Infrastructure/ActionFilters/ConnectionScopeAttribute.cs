using System;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ConnectionScopeAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = 0;

        private static TransactionScope TransactionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(HttpContextItems.TransactionScopeKey))
                {
                    return (TransactionScope)HttpContext.Current.Items[HttpContextItems.TransactionScopeKey];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[HttpContextItems.TransactionScopeKey] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(HttpContextItems.TransactionScopeKey);
                }
            }
        }

        private static QPConnectionScope ConnectionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(HttpContextItems.ConnectionScopeKey))
                {
                    return (QPConnectionScope)HttpContext.Current.Items[HttpContextItems.ConnectionScopeKey];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[HttpContextItems.ConnectionScopeKey] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(HttpContextItems.ConnectionScopeKey);
                }
            }
        }

        protected virtual ConnectionScopeMode Mode => ConnectionScopeMode.TransactionOn;

        public ConnectionScopeAttribute()
        {
            Order = FilterOrder;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as QPController;
            if (controller == null || !filterContext.HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (Mode == ConnectionScopeMode.TransactionOn)
                {
                    TransactionScope = QPConfiguration.CreateTransactionScope();
                }

                ConnectionScope = new QPConnectionScope();
            }

            base.OnActionExecuting(filterContext);
        }

        public static void DisposeScopes()
        {
            if (ConnectionScope != null)
            {
                ConnectionScope.ForcedDispose();
                ConnectionScope = null;
            }

            if (TransactionScope != null)
            {
                TransactionScope.Dispose();
                TransactionScope = null;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as QPController;
            try
            {
                if ((controller == null || !filterContext.HttpContext.IsXmlDbUpdateReplayAction())
                    && filterContext.Exception == null
                    && TransactionScope != null
                    && filterContext.Controller.ViewData.ModelState.IsValid
                    && Transaction.Current?.TransactionInformation.Status == TransactionStatus.Active)
                {
                    TransactionScope.Complete();
                }
            }
            finally
            {
                DisposeScopes();
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
