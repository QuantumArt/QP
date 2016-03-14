using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Quantumart.QP8.WebMvc.WinLogOn
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "WinLogOn", action = "Index", id = UrlParameter.Optional } // Parameter defaults
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
