using System.Linq;
using System.Web;
using System.Web.Routing;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateHttpContextProcessor : IXmlDbUpdateHttpContextProcessor
    {
        public HttpContextBase PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId, bool useGuidSubstitution)
        {
            Ensure.NotNull(QPConnectionScope.Current.DbConnection, "QPConnection scope should be initialized to use fake mvc context");
            var urlParts = recordedAction.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray()).Where(n => !string.IsNullOrEmpty(n) && n != "~").ToArray();
            var controllerName = urlParts[0];
            var controllerAction = urlParts[1];
            var requestContext = new RequestContext(
                XmlDbUpdateHttpContextHelpers.BuildHttpContextBase(recordedAction, backendUrl, userId, useGuidSubstitution),
                XmlDbUpdateHttpContextHelpers.GetRouteData(recordedAction, controllerName, controllerAction)
            );

            BackendActionContext.ResetCurrent();
            XmlDbUpdateHttpContextHelpers.BuildController(requestContext, controllerName, CultureHelpers.GetCultureInfoByLcid(recordedAction.Lcid)).Execute(requestContext);
            return requestContext.HttpContext;
        }
    }
}
