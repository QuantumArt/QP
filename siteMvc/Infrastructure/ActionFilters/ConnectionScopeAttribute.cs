using System;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

        private static TransactionScope GetTransactionScope(HttpContext context)
        {
            if (context.Items.TryGetValue(HttpContextItems.TransactionScopeKey, out object value))
            {
                return (TransactionScope)value;
            }
            return null;
        }

        private static void SetTransactionScope(HttpContext context, TransactionScope value)
        {
            if (value != null)
            {
                context.Items[HttpContextItems.TransactionScopeKey] = value;
            }
            else
            {
                context.Items.Remove(HttpContextItems.TransactionScopeKey);
            }
        }

        private static QPConnectionScope GetConnectionScope(HttpContext context)
        {
            if (context.Items.TryGetValue(HttpContextItems.ConnectionScopeKey, out object value))
            {
                return (QPConnectionScope)value;
            }
            return null;
        }

        private static void SetConnectionScope(HttpContext context, QPConnectionScope value)
        {
            if (value != null)
            {
                context.Items[HttpContextItems.ConnectionScopeKey] = value;
            }
            else
            {
                context.Items.Remove(HttpContextItems.ConnectionScopeKey);
            }
        }

        protected virtual ConnectionScopeMode Mode => ConnectionScopeMode.TransactionOn;

        public ConnectionScopeAttribute()
        {
            Order = FilterOrder;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var qpController = filterContext.Controller as QPController;
            if (qpController == null || !filterContext.HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (Mode == ConnectionScopeMode.TransactionOn)
                {
                    var transactionScope = QPConfiguration.CreateTransactionScope();
                    SetTransactionScope(filterContext.HttpContext, transactionScope);
                }

                SetConnectionScope(filterContext.HttpContext, new QPConnectionScope());
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var qpController = filterContext.Controller as QPController;
            try
            {
                if ((qpController == null || !filterContext.HttpContext.IsXmlDbUpdateReplayAction())
                    && filterContext.Exception == null
                    && ((Controller)filterContext.Controller).ViewData.ModelState.IsValid
                    && Transaction.Current?.TransactionInformation.Status == TransactionStatus.Active)
                {
                    var transactionScope = GetTransactionScope(filterContext.HttpContext);
                    if (transactionScope != null)
                    {
                        transactionScope.Complete();
                    }
                }
            }
            finally
            {
                var transactionScope = GetTransactionScope(filterContext.HttpContext);
                if (transactionScope != null)
                {
                    transactionScope.Dispose();
                    SetTransactionScope(filterContext.HttpContext, null);
                }

                var connectionScope = GetConnectionScope(filterContext.HttpContext);
                if (connectionScope != null)
                {
                    connectionScope.ForcedDispose();
                    SetConnectionScope(filterContext.HttpContext, null);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
