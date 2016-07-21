using System;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NoTransactionConnectionScopeAttribute : ConnectionScopeAttribute
    {
        protected override ConnectionScopeMode Mode => ConnectionScopeMode.TransactionOff;
    }
}
