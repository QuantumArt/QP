using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Factories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using System;
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
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "WinLogOn", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            LogProvider.LogFactory = new NLogFactory();
            Logger.Log = LogProvider.LogFactory.GetLogger(AssemblyHelpers.GetAssemblyName());
            ModelBinders.Binders.DefaultBinder = new QpModelBinder();
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exсeption = Server.GetLastError();
            if (exсeption != null)
            {
                Logger.Log.SetContext(LoggerData.HttpErrorCodeCustomVariable, new HttpException(null, exсeption).GetHttpCode());
                Logger.Log.Fatal("Application_Error", exсeption);
            }
        }
    }
}
