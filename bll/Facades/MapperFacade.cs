﻿#pragma warning disable CS0618 // Type or member is obsolete

using System.Collections.Generic;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Mappers.EntityPermissions;
using Quantumart.QP8.BLL.Mappers.VisualEditor;
using Quantumart.QP8.BLL.Mappers.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Facades
{
    public static class MapperFacade
    {
        internal static  T Create<T>(IMapperConfigurationExpression cfg, bool withDal = false)
            where T : GenericMapper, new()
        {
            var item = new T();
            item.CreateBizMapper(cfg);
            if (withDal)
            {
                item.CreateDalMapper(cfg);
            }

            return item;
        }

        public static void CreateAllMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<decimal, bool>().ConvertUsing(src => Converter.ToBoolean(src));


            cfg.CreateMap<decimal?, int?>().ConvertUsing(src => Converter.ToNullableInt32(src));
            cfg.CreateMap<bool, decimal>().ConvertUsing(src => Converter.ToDecimal(src));
            cfg.CreateMap<int, decimal>().ConvertUsing(src => src);
            cfg.CreateMap<decimal, int>().ConvertUsing(src => Converter.ToInt32(src));
            cfg.CreateMap<int?, decimal?>().ConvertUsing(src => src);

            DataRowMapper.CreateMap<SearchInArticlesResultItem>(cfg);
            DataRowMapper.CreateMap<VisualEditFieldParams>(cfg);
            DataRowMapper.CreateMap<BackendActionCacheRecord>(cfg);

            DataRowMapper.CreateMap<SchemaInfo>(cfg);
            DataRowMapper.CreateMap<RelationData>(cfg);
            DataRowMapper.CreateMap<InsertData>(cfg);
            DataRowMapper.CreateMap<ArticleInfo>(cfg);
            DataRowMapper.CreateMap<PageInfo>(cfg);

            SiteMapper = Create<SiteMapper>(cfg, true);
            UserMapper = Create<UserMapper>(cfg, true);
            ContentMapper = Create<ContentMapper>(cfg, true);
            ToolbarButtonMapper = Create<ToolbarButtonMapper>(cfg, true);
            BackendActionMapper = Create<BackendActionMapper>(cfg, true);
            BackendActionStatusMapper = Create<BackendActionStatusMapper>(cfg);
            BackendActionTypeMapper = Create<BackendActionTypeMapper>(cfg);
            ContextMenuItemRowMapper = Create<ContextMenuItemRowMapper>(cfg);
            ContextMenuRowMapper = Create<ContextMenuRowMapper>(cfg);
            EntityTypeMapper = Create<EntityTypeMapper>(cfg);
            TreeNodeMapper = Create<TreeNodeMapper>(cfg);
            ViewTypeMapper = Create<ViewTypeMapper>(cfg);
            BackendActionViewMapper = Create<BackendActionViewMapper>(cfg);
            FieldMapper = Create<FieldMapper>(cfg, true);
            FieldTypeMapper = Create<FieldTypeMapper>(cfg);
            ArticleMapper = Create<ArticleMapper>(cfg, true);
            ArticleRowMapper = Create<ArticleRowMapper>(cfg, true);
            ContainerMapper = Create<ContainerMapper>(cfg, true);
            ContentFormMapper = Create<ContentFormMapper>(cfg, true);
            ObjectValueMapper = Create<ObjectValueMapper>(cfg);
            ArticleListItemRowMapper = Create<ArticleListItemRowMapper>(cfg);
            StatusHistoryItemMapper = Create<StatusHistoryItemMapper>(cfg, true);
            StatusTypeRowMapper = Create<StatusTypeRowMapper>(cfg, true);
            ObjectFormatSearchResultRowMapper = Create<ObjectFormatSearchResultRowMapper>(cfg);
            ObjectSearchResultRowMapper = Create<ObjectSearchResultRowMapper>(cfg);
            ObjectFormatVersionRowMapper = Create<ObjectFormatVersionRowMapper>(cfg);
            ObjectFormatVersionMapper = Create<ObjectFormatVersionMapper>(cfg);
            DbMapper = Create<DbMapper>(cfg, true);
            TemplateObjectFormatDtoRowMapper = Create<TemplateObjectFormatDtoRowMapper>(cfg);
            ExternalNotificationMapper = Create<ExternalNotificationMapper>(cfg);
            SystemNotificationMapper = Create<SystemNotificationMapper>(cfg);
            XmlDbUpdateLogMapper = new XmlDbUpdateLogMapper(cfg);
            XmlDbUpdateActionsLogMapper = new XmlDbUpdateActionsLogMapper(cfg);
            ArticleScheduleMapper = Create<ArticleScheduleMapper>(cfg, true);
            ContentConstraintMapper = Create<ContentConstraintMapper>(cfg, true);
            ContentConstraintRuleMapper = Create<ContentConstraintRuleMapper>(cfg, true);
            NotificationTemplateFormatMapper = Create<NotificationTemplateFormatMapper>(cfg, true);
            ContentWorkflowBindMapper = Create<ContentWorkflowBindMapper>(cfg, true);
            ArticleWorkflowBindMapper = Create<ArticleWorkflowBindMapper>(cfg);
            ContentLinkMapper = Create<ContentLinkMapper>(cfg, true);
            ArticleVersionMapper = Create<ArticleVersionMapper>(cfg);
            SiteFolderMapper = Create<SiteFolderMapper>(cfg, true);
            ContentFolderMapper = Create<ContentFolderMapper>(cfg, true);
            ContentGroupMapper = Create<ContentGroupMapper>(cfg, true);
            DynamicImageMapper = Create<DynamicImageMapper>(cfg, true);
            ArticleScheduleTaskMapper = Create<ArticleScheduleTaskMapper>(cfg);
            WorkflowMapper = Create<WorkflowMapper>(cfg, true);
            MaskTemplateMapper = Create<MaskTemplateMapper>(cfg);
            VirtualFieldDataMapper = Create<VirtualFieldDataMapper>(cfg);
            UnionAttrMapper = Create<UnionAttrMapper>(cfg, true);
            UserQueryAttrMapper = Create<UserQueryAttrMapper>(cfg, true);
            VirtualFieldsRelationMapper = Create<VirtualFieldsRelationMapper>(cfg);
            UnionFieldRelationCountMapper = Create<UnionFieldRelationCountMapper>(cfg);
            BackendActionLogMapper = Create<BackendActionLogMapper>(cfg, true);
            ButtonTraceRowMapper = Create<ButtonTraceRowMapper>(cfg);
            RemovedEntitiesRowMapper = Create<RemovedEntitiesRowMapper>(cfg);
            BackendActionLogRowMapper = Create<BackendActionLogRowMapper>(cfg);
            ToolbarButtonRowMapper = Create<ToolbarButtonRowMapper>(cfg);
            SessionsLogMapper = Create<SessionsLogMapper>(cfg, true);
            SessionsLogRowMapper = Create<SessionsLogRowMapper>(cfg);
            CustomActionMapper = Create<CustomActionMapper>(cfg, true);
            CustomActionListItemRowMapper = Create<CustomActionListItemRowMapper>(cfg);
            ContextMenuMapper = Create<ContextMenuMapper>(cfg);
            ContextMenuItemMapper = Create<ContextMenuItemMapper>(cfg, true);
            ContentListItemRowMapper = Create<ContentListItemRowMapper>(cfg);
            UserListItemRowMapper = Create<UserListItemRowMapper>(cfg);
            UserGroupMapper = Create<UserGroupMapper>(cfg, true);
            UserGroupListItemRowMapper = Create<UserGroupListItemRowMapper>(cfg);
            NotificationMapper = Create<NotificationMapper>(cfg, true);
            NotificationListItemRowMapper = Create<NotificationListItemRowMapper>(cfg);
            VisualEditorPluginListItemRowMapper = Create<VisualEditorPluginListItemRowMapper>(cfg);
            WorkflowListItemRowMapper = Create<WorkflowListItemRowMapper>(cfg);
            VisualEditorPluginMapper = Create<VisualEditorPluginMapper>(cfg, true);
            VisualEditorCommandMapper = Create<VisualEditorCommandMapper>(cfg, true);
            EntityPermissionLevelMapper = Create<EntityPermissionLevelMapper>(cfg);
            PermissionListItemRowMapper = Create<PermissionListItemRowMapper>(cfg);
            SitePermissionMapper = Create<SitePermissionMapper>(cfg, true);
            ContentPermissionMapper = Create<ContentPermissionMapper>(cfg, true);
            ArticlePermissionMapper = Create<ArticlePermissionMapper>(cfg, true);
            WorkflowPermissionMapper = Create<WorkflowPermissionMapper>(cfg, true);
            SiteFolderPermissionMapper = Create<SiteFolderPermissionMapper>(cfg, true);
            EntityTypePermissionMapper = Create<EntityTypePermissionMapper>(cfg, true);
            BackendActionPermissionMapper = Create<BackendActionPermissionMapper>(cfg, true);
            FieldListItemRowMapper = Create<FieldListItemRowMapper>(cfg);
            SiteListItemRowMapper = Create<SiteListItemRowMapper>(cfg);
            ChildEntityPermissionListItemRowMapper = Create<ChildEntityPermissionListItemRowMapper>(cfg);
            ActionPermissionTreeNodeRowMapper = Create<ActionPermissionTreeNodeRowMapper>(cfg);
            VisualEditorCommandRowMapper = Create<VisualEditorCommandRowMapper>(cfg);
            PageTemplateRowMapper = Create<PageTemplateRowMapper>(cfg, true);
            PageTemplateSearchResultRowMapper = Create<PageTemplateSearchResultRowMapper>(cfg, true);
            PageRowMapper = Create<PageRowMapper>(cfg, true);
            ObjectRowMapper = Create<ObjectRowMapper>(cfg, true);
            VisualEditorStyleRowMapper = Create<VisualEditorStyleRowMapper>(cfg);
            CharsetMapper = Create<CharsetMapper>(cfg, true);
            VisualEditorStyleListItemRowMapper = Create<VisualEditorStyleListItemRowMapper>(cfg);
            VisualEditorStyleMapper = Create<VisualEditorStyleMapper>(cfg, true);
            StatusTypeListItemRowMapper = Create<StatusTypeListItemRowMapper>(cfg);
            WorkFlowRuleMapper = Create<WorkFlowRuleMapper>(cfg, true);
            NetLanguageMapper = Create<NetLanguageMapper>(cfg, true);
            LocaleMapper = Create<LocaleMapper>(cfg, true);
            PageTemplateMappper = Create<PageTemplateMappper>(cfg, true);
            PageMapper = Create<PageMapper>(cfg, true);
            ObjectMapper = Create<ObjectMapper>(cfg, true);
            ObjectTypeMapper = Create<ObjectTypeMapper>(cfg, true);
            ObjectFormatMapper = Create<ObjectFormatMapper>(cfg, true);
            ObjectFormatRowMapper = Create<ObjectFormatRowMapper>(cfg);
            StatusTypeMapper = Create<StatusMapper>(cfg, true);
        }

        internal static DataRowMapper DataRowMapper;
        internal static  SiteMapper SiteMapper;
        internal static UserMapper UserMapper;
        internal static ContentMapper ContentMapper;
        internal static ToolbarButtonMapper ToolbarButtonMapper;
        internal static BackendActionMapper BackendActionMapper;
        internal static BackendActionStatusMapper BackendActionStatusMapper;
        internal static BackendActionTypeMapper BackendActionTypeMapper;
        internal static ContextMenuItemRowMapper ContextMenuItemRowMapper;
        internal static ContextMenuRowMapper ContextMenuRowMapper;
        internal static StatusMapper StatusTypeMapper;
        internal static EntityTypeMapper EntityTypeMapper;
        internal static ViewTypeMapper ViewTypeMapper;
        internal static BackendActionViewMapper BackendActionViewMapper;
        internal static TreeNodeMapper TreeNodeMapper;
        internal static FieldMapper FieldMapper;
        internal static FieldTypeMapper FieldTypeMapper;
        internal static ArticleMapper ArticleMapper;
        internal static ArticleRowMapper ArticleRowMapper;
        internal static ArticleScheduleMapper ArticleScheduleMapper;
        internal static ContentConstraintMapper ContentConstraintMapper;
        internal static ContentConstraintRuleMapper ContentConstraintRuleMapper;
        internal static NotificationTemplateFormatMapper NotificationTemplateFormatMapper;
        internal static ContentWorkflowBindMapper ContentWorkflowBindMapper;
        internal static ArticleWorkflowBindMapper ArticleWorkflowBindMapper;
        internal static ContentLinkMapper ContentLinkMapper;
        internal static ArticleVersionMapper ArticleVersionMapper;
        internal static SiteFolderMapper SiteFolderMapper;
        internal static ContentFolderMapper ContentFolderMapper;
        internal static ContentGroupMapper ContentGroupMapper;
        internal static DynamicImageMapper DynamicImageMapper;
        internal static ArticleScheduleTaskMapper ArticleScheduleTaskMapper;
        internal static WorkflowMapper WorkflowMapper;
        internal static MaskTemplateMapper MaskTemplateMapper;
        internal static VirtualFieldDataMapper VirtualFieldDataMapper;
        internal static UnionAttrMapper UnionAttrMapper;
        internal static UserQueryAttrMapper UserQueryAttrMapper;
        internal static VirtualFieldsRelationMapper VirtualFieldsRelationMapper;
        internal static UnionFieldRelationCountMapper UnionFieldRelationCountMapper;
        internal static BackendActionLogMapper BackendActionLogMapper;
        internal static ButtonTraceRowMapper ButtonTraceRowMapper;
        internal static RemovedEntitiesRowMapper RemovedEntitiesRowMapper;
        internal static BackendActionLogRowMapper BackendActionLogRowMapper;
        internal static ToolbarButtonRowMapper ToolbarButtonRowMapper;
        internal static SessionsLogMapper SessionsLogMapper;
        internal static SessionsLogRowMapper SessionsLogRowMapper;
        internal static CustomActionMapper CustomActionMapper;
        internal static CustomActionListItemRowMapper CustomActionListItemRowMapper;
        internal static ContextMenuMapper ContextMenuMapper;
        internal static ContextMenuItemMapper ContextMenuItemMapper;
        internal static ContentListItemRowMapper ContentListItemRowMapper;
        internal static UserListItemRowMapper UserListItemRowMapper;
        internal static UserGroupMapper UserGroupMapper;
        internal static UserGroupListItemRowMapper UserGroupListItemRowMapper;
        internal static NotificationMapper NotificationMapper;
        internal static NotificationListItemRowMapper NotificationListItemRowMapper;
        internal static VisualEditorPluginListItemRowMapper VisualEditorPluginListItemRowMapper;
        internal static WorkflowListItemRowMapper WorkflowListItemRowMapper;
        internal static VisualEditorPluginMapper VisualEditorPluginMapper;
        internal static VisualEditorCommandMapper VisualEditorCommandMapper;
        internal static EntityPermissionLevelMapper EntityPermissionLevelMapper;
        internal static PermissionListItemRowMapper PermissionListItemRowMapper;
        internal static SitePermissionMapper SitePermissionMapper;
        internal static ContentPermissionMapper ContentPermissionMapper;
        internal static ArticlePermissionMapper ArticlePermissionMapper;
        internal static WorkflowPermissionMapper WorkflowPermissionMapper;
        internal static SiteFolderPermissionMapper SiteFolderPermissionMapper;
        internal static EntityTypePermissionMapper EntityTypePermissionMapper;
        internal static BackendActionPermissionMapper BackendActionPermissionMapper;
        internal static FieldListItemRowMapper FieldListItemRowMapper;
        internal static SiteListItemRowMapper SiteListItemRowMapper;
        internal static ChildEntityPermissionListItemRowMapper ChildEntityPermissionListItemRowMapper;
        internal static ActionPermissionTreeNodeRowMapper ActionPermissionTreeNodeRowMapper;
        internal static VisualEditorCommandRowMapper VisualEditorCommandRowMapper;
        internal static PageTemplateRowMapper PageTemplateRowMapper;
        internal static PageTemplateSearchResultRowMapper PageTemplateSearchResultRowMapper;
        internal static PageRowMapper PageRowMapper;
        internal static ObjectRowMapper ObjectRowMapper;
        internal static VisualEditorStyleRowMapper VisualEditorStyleRowMapper;
        internal static VisualEditorStyleListItemRowMapper VisualEditorStyleListItemRowMapper;
        internal static VisualEditorStyleMapper VisualEditorStyleMapper;
        internal static StatusTypeListItemRowMapper StatusTypeListItemRowMapper;
        internal static WorkFlowRuleMapper WorkFlowRuleMapper;
        internal static NetLanguageMapper NetLanguageMapper;
        internal static LocaleMapper LocaleMapper;
        internal static CharsetMapper CharsetMapper;
        internal static PageTemplateMappper PageTemplateMappper;
        internal static PageMapper PageMapper;
        internal static ObjectMapper ObjectMapper;
        internal static ObjectTypeMapper ObjectTypeMapper;
        internal static ObjectFormatMapper ObjectFormatMapper;
        internal static ObjectFormatRowMapper ObjectFormatRowMapper;
        internal static ContainerMapper ContainerMapper;
        internal static ContentFormMapper ContentFormMapper;
        internal static ObjectValueMapper ObjectValueMapper;
        internal static ArticleListItemRowMapper ArticleListItemRowMapper;
        internal static StatusHistoryItemMapper StatusHistoryItemMapper;
        internal static StatusTypeRowMapper StatusTypeRowMapper;
        internal static ObjectFormatSearchResultRowMapper ObjectFormatSearchResultRowMapper;
        internal static ObjectSearchResultRowMapper ObjectSearchResultRowMapper;
        internal static ObjectFormatVersionRowMapper ObjectFormatVersionRowMapper;
        internal static ObjectFormatVersionMapper ObjectFormatVersionMapper;
        internal static DbMapper DbMapper;
        internal static TemplateObjectFormatDtoRowMapper TemplateObjectFormatDtoRowMapper;
        internal static ExternalNotificationMapper ExternalNotificationMapper;
        internal static SystemNotificationMapper SystemNotificationMapper;
        internal static XmlDbUpdateLogMapper XmlDbUpdateLogMapper;
        internal static XmlDbUpdateActionsLogMapper XmlDbUpdateActionsLogMapper;
    }
}
