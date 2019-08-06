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

            app.UseMvc(RegisterRoutes);
        }

        private static void RegisterRoutes(IRouteBuilder routes)
        {
            // TODO: routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.MapRoute(
                "MultistepAction",
                "Multistep/{command}/{action}/{tabId}/{parentId}",
                new { controller = "Multistep", parentId = 0 },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "Properties",
                "{controller}/{action}/{tabId}/{parentId}/{id}",
                new { action = "Properties", parentId = 0 },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "New",
                "{controller}/{action}/{tabId}/{parentId}",
                new { action = "New" },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id?}",
                new { controller = "Home", action = "Index" }
            );
        }

        private static void RegisterMappings()
        {
            Mapper.Initialize(cfg =>
            {
                ViewModelMapper.CreateAllMappings(cfg);
                DTOMapper.CreateAllMappings(cfg);
                MapperFacade.CreateAllMappings(cfg);
            });
        }
    }
}
