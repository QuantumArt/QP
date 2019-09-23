using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.Audit;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.BLL.Services.MultistepActions.Assemble;
using Quantumart.QP8.BLL.Services.MultistepActions.Base;
using Quantumart.QP8.BLL.Services.MultistepActions.CopySite;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.BLL.Services.MultistepActions.Rebuild;
using Quantumart.QP8.BLL.Services.MultistepActions.Removing;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Utils.FullTextSearch;
using Quantumart.QP8.WebMvc.Controllers;
using Quantumart.QP8.WebMvc.Hubs;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Security;
using Quantumart.QP8.Configuration;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QA.Validation.Xaml.Extensions.Rules;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;

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
            var qpOptions = new QPublishingOptions();
            Configuration.Bind("Properties", qpOptions);
            services.AddSingleton(qpOptions);
            QPConfiguration.Options = qpOptions;

            var formOptions = new FormOptions();
            Configuration.Bind("Form", formOptions);
            services.AddSingleton(formOptions);

            // used by Session middleware
            services.AddDistributedMemoryCache();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(qpOptions.CookieTimeout);
                    options.LoginPath = new PathString("/Logon");
                    options.LogoutPath = new PathString("/Logon/Logout");
                    options.SlidingExpiration = true;
                });

            services.AddOptions();
            services.AddHttpContextAccessor();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(qpOptions.SessionTimeout);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services
                .AddMvc(options =>
                {
                    options.ModelBinderProviders.Insert(0, new QpModelBinderProvider());
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            // TODO: review authenticaton and CultireInfo in SignalR
            services
                .AddSignalR()
                .AddJsonProtocol(options => {
                    options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            // services
            services
                    .AddTransient<AuthenticationHelper>()
                    .AddTransient<JsLanguageHelper>()
                    .AddTransient<JsConstantsHelper>()
                    .AddSingleton<ISearchGrammarParser, IronySearchGrammarParser>()
                    .AddTransient<IStopWordList, StopWordList>()
                    .AddTransient<IArticleSearchRepository, ArticleSearchRepository>()
                    .AddTransient<ISearchInArticlesRepository, SearchInArticlesRepository>()
                    .AddTransient<ISearchInArticlesService, SearchInArticlesService>()
                    .AddTransient<IBackendActionLogRepository, AuditRepository>()
                    .AddTransient<IBackendActionLogPagesRepository, AuditRepository>()
                    .AddTransient<IButtonTracePagesRepository, AuditRepository>()
                    .AddTransient<IRemovedEntitiesPagesRepository, AuditRepository>()
                    .AddTransient<ISessionLogRepository, AuditRepository>()
                    .AddTransient<IApplicationInfoRepository, ApplicationInfoRepository>()
                    .AddTransient<IArticleRepository, ArticleRepository>()
                    .AddTransient<IContentRepository, ContentRepository>()
                    .AddTransient<IArticleSearchService, ArticleSearchService>()
                    .AddTransient<IBackendActionLogService, BackendActionLogService>()
                    .AddTransient<IButtonTraceService, ButtonTraceService>()
                    .AddTransient<IRemovedEntitiesService, RemovedEntitiesService>()
                    .AddTransient<ISessionLogService, SessionLogService>()
                    .AddTransient<ICustomActionService, CustomActionService>()
                    .AddTransient<IFieldDefaultValueService, FieldDefaultValueService>()
                    .AddTransient<IRecreateDynamicImagesService, RecreateDynamicImagesService>()
                    .AddTransient<IUserService, UserService>()
                    .AddTransient<IUserGroupService, UserGroupService>()
                    .AddTransient<IXmlDbUpdateLogRepository, XmlDbUpdateLogRepository>()
                    .AddTransient<IXmlDbUpdateActionsLogRepository, XmlDbUpdateActionsLogRepository>()
                    .AddTransient<IXmlDbUpdateLogService, XmlDbUpdateLogService>()
                    .AddTransient<IArticleService, ArticleService>()
                    .AddTransient<IContentService, ContentService>()
                    .AddTransient<IXmlDbUpdateHttpContextProcessor, XmlDbUpdateHttpContextProcessor>()
                    .AddTransient<IXmlDbUpdateActionCorrecterService, XmlDbUpdateActionCorrecterService>()
                    .AddTransient<INotificationService, NotificationService>()
                    .AddTransient<IActionPermissionTreeService, ActionPermissionTreeService>()
                    .AddTransient<ISecurityService, SecurityService>()
                    .AddTransient<ICommunicationService, CommunicationService>()
                    .AddTransient<IVisualEditorService, VisualEditorService>()
                    .AddTransient<IWorkflowService, WorkflowService>()
                    .AddTransient<IStatusTypeService, StatusTypeService>()
                    .AddTransient<IPageTemplateService, PageTemplateService>()
                    .AddTransient<IPageService, PageService>()
                    .AddTransient<IObjectService, ObjectService>()
                    .AddTransient<IFormatService, FormatService>()
                    .AddTransient<ProcessRemoteValidationIf>() //preload XAML validation
                ;

            services
                .AddTransient<WorkflowPermissionService>()
                .AddTransient<SitePermissionService>()
                .AddTransient<SiteFolderPermissionService>()
                .AddTransient<ContentPermissionService>()
                .AddTransient<ChildContentPermissionService>()
                .AddTransient<ArticlePermissionService>()
                .AddTransient<ChildArticlePermissionService>()
                .AddTransient<EntityTypePermissionService>()
                .AddTransient<EntityTypePermissionChangeService>()
                .AddTransient<ActionPermissionService>()
                .AddTransient<ActionPermissionChangeService>()
                ;

            services
                .AddTransient<ISearchGrammarParser, IronySearchGrammarParser>()
                .AddTransient<ArticleFullTextSearchQueryParser>()
                ;

            RegisterMultistepActionServices(services);
        }

        private static void RegisterMultistepActionServices(IServiceCollection services)
        {
            Type[] allTypes = typeof(IMultistepActionService).Assembly
                .GetExportedTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .ToArray();

            // multistep action services
            Dictionary<string, Type> multistepActionServiceTypes = allTypes
                .Where(type => typeof(IMultistepActionService).IsAssignableFrom(type))
                .ToDictionary(type => type.Name);

            foreach (Type type in multistepActionServiceTypes.Values)
            {
                services.AddTransient(type);
            }

            services.AddSingleton<Func<string, IMultistepActionService>>(
                provider => command => (IMultistepActionService)provider
                    .GetRequiredService(multistepActionServiceTypes[command]));

            // action codes
            Dictionary<string, Type> actionCodeTypes = allTypes
                .Where(type => typeof(IActionCode).IsAssignableFrom(type))
                .ToDictionary(type => type.Name);

            foreach (Type type in actionCodeTypes.Values)
            {
                services.AddTransient(type);
            }

            services.AddSingleton<Func<string, IActionCode>>(
                provider => command => (IActionCode)provider
                    .GetRequiredService(actionCodeTypes[command]));
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler(loggerFactory).Action);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Scripts")),
                RequestPath =  "/Scripts"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Content")),
                RequestPath =  "/Content"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "plugins")),
                RequestPath =  "/plugins"
            });

            app.UseAuthentication();
            QPContext.SetServiceProvider(provider);
            RegisterMappings();

            app.Use(async (context, next) =>
            {
                var claim = context.User.FindFirst("CultureName");
                var cultureName = claim != null ? claim.Value : QPConfiguration.Options.Globalization.DefaultCulture;
                var cultureInfo = new CultureInfo(cultureName);
                CultureInfo.CurrentUICulture = cultureInfo;
                CultureInfo.CurrentCulture = cultureInfo;

                await next.Invoke();
            });

            app.UseSession();

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
