using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QP8.Infrastructure;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateHttpContextProcessor : IXmlDbUpdateHttpContextProcessor
    {
        public HttpContext PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId, bool useGuidSubstitution)
        {
            Ensure.NotNull(QPConnectionScope.Current.DbConnection, "QPConnection scope should be initialized to use fake mvc context");
            var urlParts = recordedAction.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray())
                .Where(n => !string.IsNullOrEmpty(n) && n != "~").ToArray();
            var controller = urlParts[0];
            var action = urlParts[1];
            BackendActionContext.ResetCurrent();

            var cultureInfo = CultureHelpers.GetCultureByLcid(recordedAction.Lcid);
            CultureInfo.CurrentCulture = cultureInfo;

            var httpContext = XmlDbUpdateHttpContextHelpers.BuildHttpContext(recordedAction, backendUrl, userId, useGuidSubstitution);
            var routeData = XmlDbUpdateHttpContextHelpers.GetRouteData(recordedAction, controller, action);

            httpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature { RouteData = routeData };
            var routeContext = new RouteContext(httpContext)
            {
                RouteData = routeData
            };

            var serviceProvider = new HttpContextAccessor().HttpContext.RequestServices;
            httpContext.RequestServices = serviceProvider;
            var handler = serviceProvider.GetRequiredService<MvcRouteHandler>();

            Task.Run(async () =>
            {
                await handler.RouteAsync(routeContext);
                await routeContext.Handler(routeContext.HttpContext);
            }).Wait();

            return httpContext;
        }
    }
}
