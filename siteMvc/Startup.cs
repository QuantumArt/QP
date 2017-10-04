using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Quantumart.QP8.WebMvc;

[assembly: OwinStartup(typeof(Startup))]
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
