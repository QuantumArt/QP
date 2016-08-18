using System;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    public enum ConnectionScopeMode
    {
        TransactionOff,
        TransactionOn
    }

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
            var controller = (QPController)filterContext.Controller;
            if (controller == null || !controller.IsReplayAction())
            {
                base.OnActionExecuting(filterContext);
                if (Mode == ConnectionScopeMode.TransactionOn)
                {
                    TransactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadUncommitted
                    });
                }

                ConnectionScope = new QPConnectionScope();
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = (QPController)filterContext.Controller;
            if (controller == null || !controller.IsReplayAction())
            {
                try
                {
                    var transactionScope = TransactionScope;
                    base.OnActionExecuted(filterContext);
                    if (filterContext.Exception == null
                        && transactionScope != null
                        && filterContext.Controller.ViewData.ModelState.IsValid
                        && Transaction.Current != null
                        && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
                    {
                        transactionScope.Complete();
                    }
                }
                finally
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
            }
        }
    }
}
