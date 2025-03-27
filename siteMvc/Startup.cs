using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
using Quantumart.QP8.BLL.Services.MultistepActions.Base;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Utils.FullTextSearch;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Security;
using Quantumart.QP8.Configuration;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using QA.Configuration;
using QA.Validation.Xaml.Extensions.Rules;
using Quantumart.QP8.ArticleScheduler;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Scheduler.Notification.Providers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using A = Quantumart.QP8.BLL.Services.ArticleServices;
using S = Quantumart.QP8.ArticleScheduler;
using ContentService = Quantumart.QP8.BLL.Services.ContentServices.ContentService;
using CustomActionService = Quantumart.QP8.BLL.Services.CustomActionService;
using DbService = Quantumart.QP8.BLL.Services.DbServices.DbService;
using Quantumart.QP8.Security.Ldap;
using Quantumart.QP8.BLL.Repository.ActiveDirectory;
using Quantumart.QP8.BLL.Services.FileSynchronization;
using Quantumart.QP8.BLL.Services.KeyCloak;
using Quantumart.QP8.Configuration.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Middleware;
using Quantumart.QP8.WebMvc.Extensions.ServiceCollections;

namespace Quantumart.QP8.WebMvc
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private Logger _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _logger.Info("Starting QP");

            try
            {
                var qpOptions = new QPublishingOptions();
                Configuration.Bind("Properties", qpOptions);
                services.AddSingleton(qpOptions);
                QPConfiguration.Options = qpOptions;

                var s3Options = new S3Options();
                Configuration.Bind("S3", s3Options);
                services.AddSingleton(s3Options);

                if (!string.IsNullOrWhiteSpace(qpOptions.SessionEncryptionKeysPath))
                {
                    if (!Directory.Exists(qpOptions.SessionEncryptionKeysPath))
                    {
                        _logger.Warn("Session storage not exists on configured path {Path}", qpOptions.SessionEncryptionKeysPath);
                    }
                    else
                    {
                        services.AddDataProtection()
                           .PersistKeysToFileSystem(new DirectoryInfo(qpOptions.SessionEncryptionKeysPath));
                    }
                }

                Configuration.Bind("Validation:TextFieldTags", QPContext.TextFieldTagValidation);

                services.Configure<FormOptions>(Configuration.GetSection("Form"));

                if (qpOptions.EnableArticleScheduler)
                {
                    var schedulerOptions = new ArticleSchedulerProperties(qpOptions);
                    Configuration.Bind("ArticleScheduler", schedulerOptions);
                    services.AddSingleton(schedulerOptions);

                    services.AddHostedService<S.ArticleService>();
                }

                var commonSchedulerProperties = new CommonSchedulerProperties();

                if (qpOptions.EnableCommonScheduler)
                {
                    Configuration.Bind("CommonScheduler", commonSchedulerProperties);
                }

                services.AddSingleton(commonSchedulerProperties);
                services.AddQuartzService(commonSchedulerProperties.Name, qpOptions.EnableCommonScheduler ? Configuration.GetSection("CommonScheduler") : null);

                // used by Session middleware
                services.AddDistributedMemoryCache();

                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedProtoHeaderName = "X-FORWARDED-PROTO";
                    options.ForwardedForHeaderName = qpOptions.XForwardedForHeaderName;

                    options.ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                    // Only loopback proxies are allowed by default.
                    // Clear that restriction because forwarders are enabled by explicit
                    // configuration.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });

                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                   .AddCookie(options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(qpOptions.CookieTimeout);
                        options.LoginPath = new PathString("/Logon");
                        options.LogoutPath = new PathString("/Logon/Logout");
                        options.AccessDeniedPath = new PathString("/Logon");
                        options.SlidingExpiration = true;
                    });

                services.AddOptions();
                services.AddHttpContextAccessor();
                services.AddHttpClient();

                services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

                services.AddScoped<IUrlHelper>(x =>
                {
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
                   .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });

                services.AddAuthorization(options => { options.AddPolicy("CustomerCodeSelected", policy => policy.RequireClaim("CustomerCode")); });

                services
                   .AddTransient<AuthenticationHelper>()
                   .AddTransient<JsLanguageHelper>()
                   .AddTransient<JsConstantsHelper>()
                   .AddTransient<PathHelper>()
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
                   .AddTransient<IArticleService, A.ArticleService>()
                   .AddTransient<IContentService, ContentService>()
                   .AddTransient<ISiteService, SiteService>()
                   .AddTransient<IXmlDbUpdateHttpContextProcessor, XmlDbUpdateHttpContextProcessor>()
                   .AddTransient<IXmlDbUpdateActionCorrecterService, XmlDbUpdateActionCorrecterService>()
                   .AddTransient<INotificationService, NotificationService>()
                   .AddTransient<IActionPermissionTreeService, ActionPermissionTreeService>()
                   .AddTransient<ISecurityService, SecurityService>()
                   .AddTransient<IVisualEditorService, VisualEditorService>()
                   .AddTransient<IQpPluginService, QpPluginService>()
                   .AddTransient<ILibraryService, LibraryService>()
                   .AddTransient<IWorkflowService, WorkflowService>()
                   .AddTransient<IStatusTypeService, StatusTypeService>()
                   .AddTransient<IPageTemplateService, PageTemplateService>()
                   .AddTransient<IPageService, PageService>()
                   .AddTransient<IObjectService, ObjectService>()
                   .AddTransient<IFormatService, FormatService>()
                   .AddTransient<ProcessRemoteValidationIf>() //preload XAML validation
                   .AddTransient<ResourceDictionary>() // preload QA.Configuration
                   .AddTransient<QuartzService>()
                   .AddScoped<LogOnCredentials>()
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

                services
                   .AddTransient<IInterfaceNotificationProvider, InterfaceNotificationProvider>()
                   .AddTransient<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>()
                   .AddTransient<IExternalSystemNotificationService, ExternalSystemNotificationService>()
                   .AddTransient<ISchedulerCustomerCollection, SchedulerCustomerCollection>()
                   .AddTransient<ICommonUserService, CommonUserService>()
                   .AddTransient<ICleanSystemFoldersService, CleanSystemFoldersService>()
                   .AddTransient<ElasticCdcImportService>()
                   .AddTransient<TarantoolCdcImportService>()
                   .AddTransient<IDbService, DbService>()
                    ;

                services.RegisterExternalWorkflow(Configuration);

                if (qpOptions.ExternalAuthentication.Enabled)
                {
                    switch (qpOptions.ExternalAuthentication.ExternalAuthenticationType)
                    {
                        case ExternalAuthenticationType.ActiveDirectory:
                            services.AddOptions<LdapSettings>()
                                .Bind(Configuration.GetSection("Ldap"))
                                .ValidateDataAnnotations()
                                .ValidateOnStart();

                            services.AddSingleton<LdapConnectionFactory>();
                            services.AddSingleton<LdapHelper>();
                            services.AddScoped<ILdapIdentityManager, LdapIdentityManager>();
                            services.AddScoped<IActiveDirectoryRepository, ActiveDirectoryRepository>();
                            services.AddScoped<IUserSynchronizationService, UserSynchronizationService>();
                            services.AddSingleton<ISsoAuthService, KeyCloakAuthServiceDummy>();

                            break;
                        case ExternalAuthenticationType.KeyCloak:
                            KeyCloakSettings settings = new();
                            Configuration.GetSection(KeyCloakSettings.ConfigurationSectionName).Bind(settings);
                            services.AddSingleton(Options.Create(settings));
                            services.AddSingleton<IKeyCloakSyncService, KeyCloakService>();
                            services.AddSingleton<IKeyCloakApiHelper, KeyCloakApiHelper>();
                            services.AddSingleton<IUserSynchronizationService, KeyCloakUserSynchronizationService>();
                            services.AddHttpClient(KeyCloakSettings.HttpClientName, client => client.BaseAddress = new(settings.ApiUrl));
                            services.AddSingleton<ILdapIdentityManager, StubIdentityManager>();
                            services.AddSingleton<ISsoAuthService, KeyCloakService>();

                            break;
                        default:
                            services.AddSingleton<ILdapIdentityManager, StubIdentityManager>();
                            services.AddSingleton<IUserSynchronizationService, UserSynchronisationServiceDummy>();
                            services.AddSingleton<ISsoAuthService, KeyCloakAuthServiceDummy>();

                            if (qpOptions.ExternalAuthentication.DisableInternalAccounts)
                            {
                                _logger.ForWarnEvent()
                                    .Message("Internal accounts disabled and none of external authentication is selected. You would not be able to log in at all!")
                                    .Log();
                            }

                            break;
                    }
                }
                else
                {
                    services.AddSingleton<ILdapIdentityManager, StubIdentityManager>();
                    services.AddSingleton<IUserSynchronizationService, UserSynchronisationServiceDummy>();
                    services.AddSingleton<ISsoAuthService, KeyCloakAuthServiceDummy>();
                }

                RegisterMultistepActionServices(services);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while starting QP");

                throw;
            }
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

        public void Configure(IApplicationBuilder app, IServiceProvider provider, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler().Action);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Scripts")),
                RequestPath = "/Scripts"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Static")),
                RequestPath = "/Static"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "plugins")),
                RequestPath = "/plugins"
            });

            app.UseForwardedHeaders();
            app.UseAuthentication();
            QPContext.SetServiceProvider(provider);
            app.UseMiddleware<LoggingMiddleware>();
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

            app.UseMvc(RegisterRoutes);
        }

        private static void RegisterRoutes(IRouteBuilder routes)
        {
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
