using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;

namespace Quantumart.QP8.WebMvc.WinLogOn
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "WinLogOn", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            ModelBinders.Binders.DefaultBinder = new QpModelBinder();
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
