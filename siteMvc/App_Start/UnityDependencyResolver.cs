using System;
using System.Collections.Generic;
using System.Web.Mvc;
using QP8.Infrastructure.Helpers;
using QP8.Infrastructure.Logging.Unity;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.Audit;
using Quantumart.QP8.BLL.Services.ContentServices;
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
using Unity;
using Unity.Injection;

namespace Quantumart.QP8.WebMvc
{
    public class UnityDependencyResolver : IDependencyResolver
    {
        public IUnityContainer UnityContainer { get; }

        public UnityDependencyResolver()
        {
            UnityContainer = new UnityContainer()
                .RegisterInstance<ISearchGrammarParser>(new IronySearchGrammarParser(new StopWordList()))
                .RegisterType<IArticleSearchRepository, ArticleSearchRepository>()
                .RegisterType<ISearchInArticlesRepository, SearchInArticlesRepository>()
                .RegisterType<ISearchInArticlesService, SearchInArticlesService>()
                .RegisterType<IBackendActionLogRepository, AuditRepository>()
                .RegisterType<IBackendActionLogPagesRepository, AuditRepository>()
                .RegisterType<IButtonTracePagesRepository, AuditRepository>()
                .RegisterType<IRemovedEntitiesPagesRepository, AuditRepository>()
                .RegisterType<ISessionLogRepository, AuditRepository>()
                .RegisterType<IApplicationInfoRepository, ApplicationInfoRepository>()
                .RegisterType<IArticleRepository, ArticleRepository>()
                .RegisterType<IContentRepository, ContentRepository>()
                .RegisterType<IArticleSearchService, ArticleSearchService>()
                .RegisterType<IBackendActionLogService, BackendActionLogService>()
                .RegisterType<IButtonTraceService, ButtonTraceService>()
                .RegisterType<IRemovedEntitiesService, RemovedEntitiesService>()
                .RegisterType<ISessionLogService, SessionLogService>()
                .RegisterType<ICustomActionService, CustomActionService>()
                .RegisterType<IFieldDefaultValueService, FieldDefaultValueService>()
                .RegisterType<IRecreateDynamicImagesService, RecreateDynamicImagesService>()
                .RegisterType<IUserService, UserService>()
                .RegisterType<IUserGroupService, UserGroupService>()
                .RegisterType<IXmlDbUpdateLogRepository, XmlDbUpdateLogRepository>()
                .RegisterType<IXmlDbUpdateActionsLogRepository, XmlDbUpdateActionsLogRepository>()
                .RegisterType<IXmlDbUpdateLogService, XmlDbUpdateLogService>()
                .RegisterType<IArticleService, ArticleService>()
                .RegisterType<IContentService, ContentService>()
                .RegisterType<IXmlDbUpdateHttpContextProcessor, XmlDbUpdateHttpContextProcessor>()
                .RegisterType<IXmlDbUpdateActionCorrecterService, XmlDbUpdateActionCorrecterService>()
                .RegisterType<ClearContentController>(new InjectionFactory(c => new ClearContentController(new ClearContentService())))
                .RegisterType<RemoveContentController>(new InjectionFactory(c => new RemoveContentController(new RemoveContentService())))
                .RegisterType<ImportArticlesController>(new InjectionFactory(c => new ImportArticlesController(new ImportArticlesService())))
                .RegisterType<ExportArticlesController>(new InjectionFactory(c => new ExportArticlesController(new ExportArticlesService())))
                .RegisterType<ExportSelectedArticlesController>(new InjectionFactory(c => new ExportSelectedArticlesController(new ExportArticlesService())))
                .RegisterType<ExportSelectedArchiveArticlesController>(new InjectionFactory(c => new ExportSelectedArchiveArticlesController(new ExportArticlesService())))
                .RegisterType<MultistepController>(new InjectionFactory(c => new MultistepController(c.Resolve<Func<string, IMultistepActionService>>(), c.Resolve<Func<string, string>>())))
                .RegisterType<CopySiteController>(new InjectionFactory(c => new CopySiteController(new CopySiteService())))
                .RegisterType<RemoveSiteController>(new InjectionFactory(c => new RemoveSiteController(new RemoveSiteService())))
                .RegisterType<AssembleSiteController>(new InjectionFactory(c => new AssembleSiteController(new AssembleSiteService())))
                .RegisterType<AssembleTemplateBaseController>(new InjectionFactory(c => new AssembleTemplateBaseController(new AssembleTemplateService())))
                .RegisterType<AssembleTemplateFromFormatController>(new InjectionFactory(c => new AssembleTemplateFromFormatController(new AssembleTemplateService())))
                .RegisterType<AssembleTemplateFromObjectController>(new InjectionFactory(c => new AssembleTemplateFromObjectController(new AssembleTemplateService())))
                .RegisterType<AssembleTemplateFromObjectListController>(new InjectionFactory(c => new AssembleTemplateFromObjectListController(new AssembleTemplateService())))
                .RegisterType<RebuildVirtualContentsController>(new InjectionFactory(c => new RebuildVirtualContentsController(new RebuildVirtualContentsService())))
                .RegisterType<VisualEditorPluginController>(new InjectionFactory(c => new VisualEditorPluginController(new VisualEditorService())))
                .RegisterType<VisualEditorStyleController>(new InjectionFactory(c => new VisualEditorStyleController(new VisualEditorService())))
                .RegisterType<SitePermissionController>(new InjectionFactory(c => new SitePermissionController(new SitePermissionService())))
                .RegisterType<ContentPermissionController>(new InjectionFactory(c => new ContentPermissionController(new ContentPermissionService(), new ChildContentPermissionService())))
                .RegisterType<ArticlePermissionController>(new InjectionFactory(c => new ArticlePermissionController(new ArticlePermissionService(), new ChildArticlePermissionService())))
                .RegisterType<SiteFolderPermissionController>(new InjectionFactory(c => new SiteFolderPermissionController(new SiteFolderPermissionService())))
                .RegisterType<EntityTypePermissionController>(new InjectionFactory(c => new EntityTypePermissionController(new EntityTypePermissionService(), new EntityTypePermissionChangeService())))
                .RegisterType<ActionPermissionController>(new InjectionFactory(c => new ActionPermissionController(new ActionPermissionService(), new ActionPermissionChangeService())))
                .RegisterType<WorkflowController>(new InjectionFactory(c => new WorkflowController(new WorkflowService())))
                .RegisterType<StatusTypeController>(new InjectionFactory(c => new StatusTypeController(new StatusTypeService())))
                .RegisterType<CssController>(new InjectionFactory(c => new CssController(new StatusTypeService())))
                .RegisterType<WorkflowPermissionController>(new InjectionFactory(c => new WorkflowPermissionController(new WorkflowPermissionService())))
                .RegisterType<PageTemplateController>(new InjectionFactory(c => new PageTemplateController(new PageTemplateService())))
                .RegisterType<PageController>(new InjectionFactory(c => new PageController(new PageService())))
                .RegisterType<ObjectController>(new InjectionFactory(c => new ObjectController(new ObjectService())))
                .RegisterType<FormatController>(new InjectionFactory(c => new FormatController(new FormatService())))
                .RegisterType<INotificationService, NotificationService>()
                .RegisterType<IActionPermissionTreeService, ActionPermissionTreeService>()
                .RegisterType<ISecurityService, SecurityService>()
                .RegisterType<ICommunicationService, CommunicationService>()
                .RegisterType<SingleUserModeHub>();

            RegisterMultistepActionServices(UnityContainer);
            UnityContainer.AddExtension(new NLogUnityContainerExtension(AssemblyHelpers.GetAssemblyName()));
        }

        private static void RegisterMultistepActionServices(IUnityContainer container)
        {
            var a = typeof(IMultistepActionService).Assembly;
            foreach (var t in a.GetExportedTypes())
            {
                if (typeof(IMultistepActionService).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                {
                    container.RegisterType(typeof(IMultistepActionService), t, t.Name);
                }

                if (typeof(IActionCode).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                {
                    container.RegisterType(typeof(IActionCode), t, t.Name);
                }
            }

            container.RegisterType<Func<string, IMultistepActionService>>(new InjectionFactory(c => new Func<string, IMultistepActionService>(command => c.Resolve<IMultistepActionService>(command))));
            container.RegisterType<Func<string, string>>(new InjectionFactory(c => new Func<string, string>(command => c.Resolve<IActionCode>(command).ActionCode)));
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return UnityContainer.Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return UnityContainer.ResolveAll(serviceType);
            }
            catch
            {
                return new List<object>();
            }
        }
    }
}
