using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    public enum ConnectionScopeMode
    {
        TransactionOff,
        TransactionOn
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ConnectionScopeAttribute : ActionFilterAttribute, IActionFilter
    {
        public static readonly int FilterOrder = 0;

        private static readonly string TRANSACTION_SCOPE_KEY = "ConnectionScopeAttribute.TransactionScope";
        private TransactionScope _TransactionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(TRANSACTION_SCOPE_KEY))
                {
                    return (TransactionScope)HttpContext.Current.Items[TRANSACTION_SCOPE_KEY];
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[TRANSACTION_SCOPE_KEY] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(TRANSACTION_SCOPE_KEY);
                }
            }
        }

        private static readonly string CONNECTION_SCOPE_KEY = "ConnectionScopeAttribute.ConnectionScope";
        private QPConnectionScope _ConnectionScope
        {
            get
            {
                if (HttpContext.Current.Items.Contains(CONNECTION_SCOPE_KEY))
                {
                    return (QPConnectionScope)HttpContext.Current.Items[CONNECTION_SCOPE_KEY];
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    HttpContext.Current.Items[CONNECTION_SCOPE_KEY] = value;
                }
                else
                {
                    HttpContext.Current.Items.Remove(CONNECTION_SCOPE_KEY);
                }
            }
        }

        protected virtual ConnectionScopeMode _Mode
        {
            get
            {
                return ConnectionScopeMode.TransactionOn;
            }
        }

        public ConnectionScopeAttribute() : this(ConnectionScopeMode.TransactionOff) { }

        public ConnectionScopeAttribute(ConnectionScopeMode mode)
        {
            Order = FilterOrder;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            QPController controller = (QPController)filterContext.Controller;
            if (controller == null || !controller.IsReplayAction())
            {
                base.OnActionExecuting(filterContext);
                if (_Mode == ConnectionScopeMode.TransactionOn)
                {
                    _TransactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadUncommitted
                    });
                }

                _ConnectionScope = new QPConnectionScope();
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            QPController controller = (QPController)filterContext.Controller;
            if (controller == null || !controller.IsReplayAction())
            {

                try
                {
                    TransactionScope transactionScope = _TransactionScope;
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
                    if (_ConnectionScope != null)
                    {
                        _ConnectionScope.ForcedDispose();
                        _ConnectionScope = null;
                    }

                    if (_TransactionScope != null)
                    {
                        _TransactionScope.Dispose();
                        _TransactionScope = null;
                    }
                }
            }
        }
    }
}