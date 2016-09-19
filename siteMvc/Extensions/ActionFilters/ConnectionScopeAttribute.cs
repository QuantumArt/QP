using System;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ConnectionScopeAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = 0;

        private const string TransactionScopeKey = "ConnectionScopeAttribute.TransactionScope";

        private static TransactionScope TransactionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(TransactionScopeKey))
                {
                    return (TransactionScope)HttpContext.Current.Items[TransactionScopeKey];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[TransactionScopeKey] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(TransactionScopeKey);
                }
            }
        }

        private const string ConnectionScopeKey = "ConnectionScopeAttribute.ConnectionScope";

        private static QPConnectionScope ConnectionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(ConnectionScopeKey))
                {
                    return (QPConnectionScope)HttpContext.Current.Items[ConnectionScopeKey];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[ConnectionScopeKey] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(ConnectionScopeKey);
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
            if ((controller == null) || !controller.IsReplayAction())
            {
                if (Mode == ConnectionScopeMode.TransactionOn)
                {
                    TransactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadUncommitted
                    });
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
            if ((controller == null) || !controller.IsReplayAction())
            {
                try
                {
                    if ((filterContext.Exception == null) && (TransactionScope != null)
                        && filterContext.Controller.ViewData.ModelState.IsValid
                        && (Transaction.Current?.TransactionInformation.Status == TransactionStatus.Active))
                    {
                        TransactionScope.Complete();
                    }
                }
                finally
                {
                    DisposeScopes();
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
