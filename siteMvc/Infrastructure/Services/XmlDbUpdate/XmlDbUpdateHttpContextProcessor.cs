using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
//using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateHttpContextProcessor : IXmlDbUpdateHttpContextProcessor
    {
        public HttpContext PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId, bool useGuidSubstitution)
        {
            // Ensure.NotNull(QPConnectionScope.Current.DbConnection, "QPConnection scope should be initialized to use fake mvc context");
            // var urlParts = recordedAction.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray()).Where(n => !string.IsNullOrEmpty(n) && n != "~").ToArray();
            // var controllerName = urlParts[0];
            // var controllerAction = urlParts[1];
            // var requestContext = new RequestContext(
            //     XmlDbUpdateHttpContextHelpers.BuildHttpContextBase(recordedAction, backendUrl, userId, useGuidSubstitution),
            //     XmlDbUpdateHttpContextHelpers.GetRouteData(recordedAction, controllerName, controllerAction)
            // );
            //
            // BackendActionContext.ResetCurrent();
            // XmlDbUpdateHttpContextHelpers.BuildController(requestContext, controllerName, CultureHelpers.GetCultureByLcid(recordedAction.Lcid)).Execute(requestContext);
            // return requestContext.HttpContext;
            return null;
        }
    }
}
