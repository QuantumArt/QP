using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.DAL
{
    public abstract class QPModelDataContext : DbContext
    {

        protected readonly string _nameOrConnectionString;
        protected readonly DbConnection _connection;

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[] {new NLogLoggerProvider()});

        protected QPModelDataContext()
        {
        }

        protected QPModelDataContext(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        protected QPModelDataContext(DbConnection dbConnection)
        {

            _connection = dbConnection;
        }


        public DbSet<ActionTypeDAL> ActionTypeSet { get; set; }
        public DbSet<ArticleWorkflowBindDAL> ArticleWorkflowBindSet { get; set; }
        public DbSet<FieldTypeDAL> FieldTypeSet { get; set; }
        public DbSet<BackendActionDAL> BackendActionSet { get; set; }
        public DbSet<BackendActionLogDAL> BackendActionLogSet { get; set; }
        public DbSet<CharsetDAL> CharsetSet { get; set; }
        public DbSet<CodeSnippetDAL> CodeSnippetSet { get; set; }
        public DbSet<ContainerDAL> ContainerSet { get; set; }
        public DbSet<ContentDAL> ContentSet { get; set; }
        public DbSet<ContentPermissionDAL> ContentPermissionSet { get; set; }
        public DbSet<FieldDAL> FieldSet { get; set; }
        public DbSet<ContentConstraintDAL> ContentConstraintSet { get; set; }
        public DbSet<ContentDataDAL> ContentDataSet { get; set; }
        public DbSet<ContentFolderDAL> ContentFolderSet { get; set; }
        public DbSet<ContentFolderAccessDAL> ContentFolderAccessSet { get; set; }
        public DbSet<ContentFormDAL> ContentFormSet { get; set; }
        public DbSet<ContentGroupDAL> ContentGroupSet { get; set; }
        public DbSet<ArticleDAL> ArticleSet { get; set; }
        public DbSet<ArticlePermissionDAL> ArticlePermissionSet { get; set; }
        public DbSet<ArticleScheduleDAL> ArticleScheduleSet { get; set; }
        public DbSet<ArticleStatusHistoryDAL> ArticleStatusHistorySet { get; set; }
        public DbSet<ArticleVersionDAL> ArticleVersionSet { get; set; }
        public DbSet<ContentToContentDAL> ContentToContentSet { get; set; }
        public DbSet<ContentWorkflowBindDAL> ContentWorkflowBindSet { get; set; }
        public DbSet<DangerousActionsDAL> DangerousActionsSet { get; set; }
        public DbSet<DeveloperDAL> DeveloperSet { get; set; }
        public DbSet<DynamicImageFieldDAL> DynamicImageFieldSet { get; set; }
        public DbSet<EntityTypeDAL> EntityTypeSet { get; set; }
        public DbSet<SiteFolderDAL> SiteFolderSet { get; set; }
        public DbSet<SiteFolderPermissionDAL> SiteFolderPermissionSet { get; set; }
        public DbSet<ItemToItemDAL> ItemToItemSet { get; set; }
        public DbSet<ItemToItemVersionDAL> ItemToItemVersionSet { get; set; }
        public DbSet<LanguagesDAL> LanguagesSet { get; set; }
        public DbSet<LocaleDAL> LocaleSet { get; set; }
        public DbSet<MaskTemplateDAL> MaskTemplateSet { get; set; }
        public DbSet<NetLanguagesDAL> NetLanguagesSet { get; set; }
        public DbSet<NotificationsDAL> NotificationsSet { get; set; }
        public DbSet<NotificationsSentDAL> NotificationsSentSet { get; set; }
        public DbSet<ObjectDAL> ObjectSet { get; set; }
        public DbSet<ObjectFormatDAL> ObjectFormatSet { get; set; }
        public DbSet<ObjectFormatVersionDAL> ObjectFormatVersionSet { get; set; }
        public DbSet<ObjectTypeDAL> ObjectTypeSet { get; set; }
        public DbSet<ObjectValuesDAL> ObjectValuesSet { get; set; }
        public DbSet<PageDAL> PageSet { get; set; }
        public DbSet<PageTemplateDAL> PageTemplateSet { get; set; }
        public DbSet<PageTraceDAL> PageTraceSet { get; set; }
        public DbSet<PageTraceFormatDAL> PageTraceFormatSet { get; set; }
        public DbSet<PageTraceFormatValuesDAL> PageTraceFormatValuesSet { get; set; }
        public DbSet<PermissionLevelDAL> PermissionLevelSet { get; set; }
        public DbSet<RemovedEntitiesDAL> RemovedEntitiesSet { get; set; }
        public DbSet<RemovedFilesDAL> RemovedFilesSet { get; set; }
        public DbSet<SessionsLogDAL> SessionsLogSet { get; set; }
        public DbSet<SiteDAL> SiteSet { get; set; }
        public DbSet<SitePermissionDAL> SitePermissionSet { get; set; }
        public DbSet<StatusTypeDAL> StatusTypeSet { get; set; }
        public DbSet<StyleDAL> StyleSet { get; set; }
        public DbSet<StyleAttributeDAL> StyleAttributeSet { get; set; }
        public DbSet<StyleTagDAL> StyleTagSet { get; set; }
        public DbSet<SystemStatusTypeDAL> SystemStatusTypeSet { get; set; }
        public DbSet<TodayPanelsDAL> TodayPanelsSet { get; set; }
        public DbSet<TranslationsDAL> TranslationsSet { get; set; }
        public DbSet<UnionContentsDAL> UnionContentsSet { get; set; }
        public DbSet<UserGroupDAL> UserGroupSet { get; set; }
        public DbSet<UserQueryAttrsDAL> UserQueryAttrsSet { get; set; }
        public DbSet<UserQueryContentsDAL> UserQueryContentsSet { get; set; }
        public DbSet<UserToPanelDAL> UserToPanelSet { get; set; }
        public DbSet<UserDAL> UserSet { get; set; }
        public DbSet<VersionContentDataDAL> VersionContentDataSet { get; set; }
        public DbSet<WaitingForApprovalDAL> WaitingForApprovalSet { get; set; }
        public DbSet<WorkflowDAL> WorkflowSet { get; set; }
        public DbSet<WorkflowPermissionDAL> WorkflowPermissionSet { get; set; }
        public DbSet<WorkflowRulesDAL> WorkflowRulesSet { get; set; }
        public DbSet<ActionViewDAL> ActionViewSet { get; set; }
        public DbSet<ViewTypeDAL> ViewTypeSet { get; set; }
        public DbSet<ToolbarButtonDAL> ToolbarButtonSet { get; set; }
        public DbSet<UnionAttrDAL> UnionAttrSet { get; set; }
        public DbSet<ContentConstraintRuleDAL> ContentConstraintRuleSet { get; set; }
        public DbSet<CustomActionDAL> CustomActionSet { get; set; }
        public DbSet<ContextMenuDAL> ContextMenuSet { get; set; }
        public DbSet<ContextMenuItemDAL> ContextMenuItemSet { get; set; }
        public DbSet<BackendActionPermissionDAL> BackendActionPermissionSet { get; set; }
        public DbSet<EntityTypePermissionDAL> EntityTypePermissionSet { get; set; }
        public DbSet<VeCommandDAL> VeCommandSet { get; set; }
        public DbSet<VePluginDAL> VePluginSet { get; set; }
        public DbSet<VeStyleDAL> VeStyleSet { get; set; }
        public DbSet<PluginDAL> PluginSet { get; set; }

        public DbSet<PluginFieldDAL> PluginFieldSet { get; set; }

        public DbSet<PluginFieldValueDAL> PluginFieldValueSet { get; set; }
        public DbSet<PluginVersionDAL> PluginVersionSet { get; set; }

        public DbSet<UserDefaultFilterItemDAL> UserDefaultFilterSet { get; set; }
        public DbSet<DbDAL> DbSet { get; set; }
        public DbSet<AppSettingsDAL> AppSettingsSet { get; set; }
        public DbSet<ExternalNotificationDAL> ExternalNotificationSet { get; set; }
        public DbSet<XmlDbUpdateLogEntity> XML_DB_UPDATE { get; set; }
        public DbSet<XmlDbUpdateActionsLogEntity> XML_DB_UPDATE_ACTIONS { get; set; }
        public DbSet<CdcLastExecutedLsn> CdcLastExecutedLsn { get; set; }
        public DbSet<SystemNotificationDAL> SystemNotificationSet { get; set; }
        public DbSet<BackendActionLogUserGroupDAL> BackendActionLogUserRoleSet { get; set; }
        public DbSet<ExternalWorkflowDAL> ExternalWorkflowSet { get; set; }
        public DbSet<ExternalWorkflowStatusDAL> ExternalWorkflowStatusSet { get; set; }
        public DbSet<ExternalWorkflowInProgressDAL> ExternalWorkflowInProgressSet { get; set; }

        public static readonly string CountColumn = "ROWS_COUNT";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLoggerFactory(MyLoggerFactory);

            // Database.SetCommandTimeout(SqlCommandFactory.CommandTimeout);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
