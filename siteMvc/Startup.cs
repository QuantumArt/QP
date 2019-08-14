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
            // used by Session middleware
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
            });

            services
                .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
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
                .AddTransient<IFormatService, FormatService>();

            // multistep action controllers
            services
                .AddTransient(provider => new ClearContentController(new ClearContentService()))
                .AddTransient(provider => new RemoveContentController(new RemoveContentService()))
                .AddTransient(provider => new ImportArticlesController(new ImportArticlesService()))
                .AddTransient(provider => new ExportArticlesController(new ExportArticlesService()))
                .AddTransient(provider => new ExportSelectedArticlesController(new ExportArticlesService()))
                .AddTransient(provider => new ExportSelectedArchiveArticlesController(new ExportArticlesService()))
                .AddTransient(provider => new CopySiteController(new CopySiteService()))
                .AddTransient(provider => new RemoveSiteController(new RemoveSiteService()))
                .AddTransient(provider => new AssembleSiteController(new AssembleSiteService()))
                .AddTransient(provider => new AssembleTemplateBaseController(new AssembleTemplateService()))
                .AddTransient(provider => new AssembleTemplateFromFormatController(new AssembleTemplateService()))
                .AddTransient(provider => new AssembleTemplateFromObjectController(new AssembleTemplateService()))
                .AddTransient(provider => new AssembleTemplateFromObjectListController(new AssembleTemplateService()))
                .AddTransient(provider => new RebuildVirtualContentsController(new RebuildVirtualContentsService()));

            // permission controllers
            services
                .AddTransient(provider => new WorkflowPermissionController(new WorkflowPermissionService()))
                .AddTransient(provider => new SitePermissionController(new SitePermissionService()))
                .AddTransient(provider => new SiteFolderPermissionController(new SiteFolderPermissionService()))
                .AddTransient(provider => new ContentPermissionController(
                    new ContentPermissionService(), new ChildContentPermissionService()))
                .AddTransient(provider => new ArticlePermissionController(
                    new ArticlePermissionService(), new ChildArticlePermissionService()))
                .AddTransient(provider => new EntityTypePermissionController(
                    new EntityTypePermissionService(), new EntityTypePermissionChangeService()))
                .AddTransient(provider => new ActionPermissionController(
                    new ActionPermissionService(), new ActionPermissionChangeService()));

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

        public void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            QPContext.SetServiceProvider(provider);

            RegisterMappings();

            // TODO: implement exception handler
            //app.UseExceptionHandler();

            // TODO: review static files
            app.UseStaticFiles("/Content");
            app.UseStaticFiles("/Scripts");

            //app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                string cultureName = context.User.Identity is QpIdentity userIdentity
                    ? userIdentity.CultureName
                    : QPConfiguration.AppConfigSection.Globalization.DefaultCulture;

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
