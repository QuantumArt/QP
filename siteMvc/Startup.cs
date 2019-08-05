using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quantumart.QP8.WebMvc.Hubs;
using System.IO;

namespace Quantumart.QP8.WebMvc
{
    public class Startup
    {
        //public void Configuration(IAppBuilder app)
        //{
        //    app.MapSignalR();
        //    GlobalHost.HubPipeline.RequireAuthentication();
        //}

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // TODO: review Shared/Error.cshtml
                app.UseExceptionHandler("/Error");
            }

            // TODO: review static files
            app.UseStaticFiles("/Content");
            app.UseStaticFiles("/Scripts");

            app.UseSignalR(routes =>
            {
                routes.MapHub<CommunicationHub>("/signalr/communication");
                routes.MapHub<SingleUserModeHub>("/signalr/singleUserMode");
            });

            app.UseMvc();
        }
    }
}
