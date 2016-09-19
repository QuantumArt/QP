using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Quantumart.QP8.WebMvc.Startup))]
namespace Quantumart.QP8.WebMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            GlobalHost.HubPipeline.RequireAuthentication();
        }
    }
}
