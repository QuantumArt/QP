using System;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NoTransactionConnectionScopeAttribute : ConnectionScopeAttribute
    {
        protected override ConnectionScopeMode Mode => ConnectionScopeMode.TransactionOff;
    }
}
