using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    public class ExternalWorkflowCustomActionAttribute : TypeFilterAttribute
    {
        public ExternalWorkflowCustomActionAttribute() : base(typeof(ExternalWorkflowCustomActionFilter))
        {

        }

        private class ExternalWorkflowCustomActionFilter : IAuthorizationFilter
        {
            private const string CustomerCodeField = "customerCode";

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!context.HttpContext.Request.Form.TryGetValue(CustomerCodeField, out StringValues customerCode)
                    || string.IsNullOrWhiteSpace(customerCode.Single()))
                {
                    context.Result = new ObjectResult("Unable to find customer code in query parameters.")
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                    return;
                }

                QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(customerCode.Single());
                context.HttpContext.Items.Add(HttpContextItems.CurrentDbConnectionStringKey, cnnInfo);
            }
        }
    }
}
