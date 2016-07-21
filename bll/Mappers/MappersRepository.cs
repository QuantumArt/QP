using AutoMapper;
using Quantumart.QP8.BLL.Mappers.EntityPermissions;
using Quantumart.QP8.BLL.Mappers.VisualEditor;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.VisualEditor;
using System.Collections.Generic;
using System.Data;

namespace Quantumart.QP8.BLL.Mappers
{
    public class MappersRepository
    {
        internal static T Create<T>() where T : GenericMapper, new()
        {
            return Create<T>(false);
        }

        internal static T Create<T>(bool withDal) where T : GenericMapper, new()
        {
            var item = new T();
            item.CreateBizMapper();
            if (withDal)
            {
                item.CreateDalMapper();
            }

            return item;
        }

        static MappersRepository()
        {
            Mapper.CreateMap<decimal, bool>().ConvertUsing(src => Utils.Converter.ToBoolean(src));
            Mapper.CreateMap<decimal?, int?>().ConvertUsing(src => Utils.Converter.ToNullableInt32(src));
            Mapper.CreateMap<bool, decimal>().ConvertUsing(src => Utils.Converter.ToDecimal(src));
            Mapper.CreateMap<int, decimal>().ConvertUsing(src => (decimal)src);
            Mapper.CreateMap<decimal, int>().ConvertUsing(src => Utils.Converter.ToInt32(src));
            Mapper.CreateMap<int?, decimal?>().ConvertUsing(src => Utils.Converter.ToNullableDecimal(src));

            Mapper.CreateMap<IDataReader, IEnumerable<SearchInArticlesResultItem>>();
            Mapper.CreateMap<IDataReader, IEnumerable<VisualEditFieldParams>>();
            Mapper.CreateMap<IDataReader, IEnumerable<BackendActionCacheRecord>>();

            DataRowMapper.CreateMap<SchemaInfo>();
            DataRowMapper.CreateMap<RelationData>();
            DataRowMapper.CreateMap<InsertData>();
            DataRowMapper.CreateMap<ArticleInfo>();
            DataRowMapper.CreateMap<PageInfo>();
        }

        internal static readonly SiteMapper SiteMapper = Create<SiteMapper>(true);

        internal static readonly UserMapper UserMapper = Create<UserMapper>(true);

        internal static readonly ContentMapper ContentMapper = Create<ContentMapper>(true);

        internal static readonly ToolbarButtonMapper ToolbarButtonMapper = Create<ToolbarButtonMapper>(true);

        internal static readonly BackendActionMapper BackendActionMapper = Create<BackendActionMapper>(true);

        internal static readonly BackendActionStatusMapper BackendActionStatusMapper = Create<BackendActionStatusMapper>();

        internal static readonly BackendActionTypeMapper BackendActionTypeMapper = Create<BackendActionTypeMapper>();

        internal static readonly ContextMenuItemRowMapper ContextMenuItemRowMapper = Create<ContextMenuItemRowMapper>();

        internal static readonly ContextMenuRowMapper ContextMenuRowMapper = Create<ContextMenuRowMapper>();

        internal static readonly EntityTypeMapper EntityTypeMapper = Create<EntityTypeMapper>();

        internal static readonly ViewTypeMapper ViewTypeMapper = Create<ViewTypeMapper>();

        internal static readonly BackendActionViewMapper BackendActionViewMapper = Create<BackendActionViewMapper>();

        internal static readonly TreeNodeMapper TreeNodeMapper = Create<TreeNodeMapper>();

        internal static readonly FieldMapper FieldMapper = Create<FieldMapper>(true);

        internal static readonly FieldTypeMapper FieldTypeMapper = Create<FieldTypeMapper>();

        internal static readonly ArticleMapper ArticleMapper = Create<ArticleMapper>(true);

        internal static readonly ArticleRowMapper ArticleRowMapper = Create<ArticleRowMapper>(true);

        internal static readonly ArticleScheduleMapper ArticleScheduleMapper = Create<ArticleScheduleMapper>(true);

        internal static readonly StatusMapper StatusTypeMapper = Create<StatusMapper>(true);

        internal static readonly ContentConstraintMapper ContentConstraintMapper = Create<ContentConstraintMapper>(true);

        internal static readonly NotificationTemplateFormatMapper NotificationTemplateFormatMapper = Create<NotificationTemplateFormatMapper>(true);

        internal static readonly ContentConstraintRuleMapper ContentConstraintRuleMapper = Create<ContentConstraintRuleMapper>(true);

        internal static readonly ContentWorkflowBindMapper ContentWorkflowBindMapper = Create<ContentWorkflowBindMapper>(true);

        internal static readonly ArticleWorkflowBindMapper ArticleWorkflowBindMapper = Create<ArticleWorkflowBindMapper>();

        internal static readonly ContentLinkMapper ContentLinkMapper = Create<ContentLinkMapper>(true);

        internal static readonly ArticleVersionMapper ArticleVersionMapper = Create<ArticleVersionMapper>();

        internal static readonly SiteFolderMapper SiteFolderMapper = Create<SiteFolderMapper>(true);

        internal static readonly ContentFolderMapper ContentFolderMapper = Create<ContentFolderMapper>(true);

        internal static readonly ContentGroupMapper ContentGroupMapper = Create<ContentGroupMapper>(true);

        internal static readonly DynamicImageMapper DynamicImageMapper = Create<DynamicImageMapper>(true);

        internal static readonly ArticleScheduleTaskMapper ArticleScheduleTaskMapper = Create<ArticleScheduleTaskMapper>();

        internal static readonly WorkflowMapper WorkflowMapper = Create<WorkflowMapper>(true);

        internal static readonly MaskTemplateMapper MaskTemplateMapper = Create<MaskTemplateMapper>();

        internal static readonly VirtualFieldDataMapper VirtualFieldDataMapper = Create<VirtualFieldDataMapper>();

        internal static readonly UnionAttrMapper UnionAttrMapper = Create<UnionAttrMapper>(true);

        internal static readonly UserQueryAttrMapper UserQueryAttrMapper = Create<UserQueryAttrMapper>(true);

        internal static readonly VirtualFieldsRelationMapper VirtualFieldsRelationMapper = Create<VirtualFieldsRelationMapper>();

        internal static readonly UnionFieldRelationCountMapper UnionFieldRelationCountMapper = Create<UnionFieldRelationCountMapper>();

        internal static readonly BackendActionLogMapper BackendActionLogMapper = Create<BackendActionLogMapper>(true);

        internal static readonly ButtonTraceRowMapper ButtonTraceRowMapper = Create<ButtonTraceRowMapper>();

        internal static readonly RemovedEntitiesRowMapper RemovedEntitiesRowMapper = Create<RemovedEntitiesRowMapper>();

        internal static readonly BackendActionLogRowMapper BackendActionLogRowMapper = Create<BackendActionLogRowMapper>();

        internal static readonly ToolbarButtonRowMapper ToolbarButtonRowMapper = Create<ToolbarButtonRowMapper>();

        internal static readonly SessionsLogMapper SessionsLogMapper = Create<SessionsLogMapper>(true);

        internal static readonly SessionsLogRowMapper SessionsLogRowMapper = Create<SessionsLogRowMapper>();

        internal static readonly CustomActionMapper CustomActionMapper = Create<CustomActionMapper>(true);

        internal static readonly CustomActionListItemRowMapper CustomActionListItemRowMapper = Create<CustomActionListItemRowMapper>();

        internal static readonly ContextMenuMapper ContextMenuMapper = Create<ContextMenuMapper>();

        internal static readonly ContextMenuItemMapper ContextMenuItemMapper = Create<ContextMenuItemMapper>(true);

        internal static readonly ContentListItemRowMapper ContentListItemRowMapper = Create<ContentListItemRowMapper>();

        internal static readonly UserListItemRowMapper UserListItemRowMapper = Create<UserListItemRowMapper>();

        internal static readonly UserGroupMapper UserGroupMapper = Create<UserGroupMapper>(true);

        internal static readonly UserGroupListItemRowMapper UserGroupListItemRowMapper = Create<UserGroupListItemRowMapper>();

        internal static readonly NotificationMapper NotificationMapper = Create<NotificationMapper>(true);

        internal static readonly NotificationListItemRowMapper NotificationListItemRowMapper = Create<NotificationListItemRowMapper>();

        internal static readonly VisualEditorPluginListItemRowMapper VisualEditorPluginListItemRowMapper = Create<VisualEditorPluginListItemRowMapper>();

        internal static readonly WorkflowListItemRowMapper WorkflowListItemRowMapper = Create<WorkflowListItemRowMapper>();

        internal static readonly VisualEditorPluginMapper VisualEditorPluginMapper = Create<VisualEditorPluginMapper>(true);

        internal static readonly VisualEditorCommandMapper VisualEditorCommandMapper = Create<VisualEditorCommandMapper>(true);

        internal static readonly EntityPermissionLevelMapper EntityPermissionLevelMapper = Create<EntityPermissionLevelMapper>();

        internal static readonly PermissionListItemRowMapper PermissionListItemRowMapper = Create<PermissionListItemRowMapper>();

        internal static readonly SitePermissionMapper SitePermissionMapper = Create<SitePermissionMapper>(true);

        internal static readonly ContentPermissionMapper ContentPermissionMapper = Create<ContentPermissionMapper>(true);

        internal static readonly ArticlePermissionMapper ArticlePermissionMapper = Create<ArticlePermissionMapper>(true);

        internal static readonly WorkflowPermissionMapper WorkflowPermissionMapper = Create<WorkflowPermissionMapper>(true);

        internal static readonly SiteFolderPermissionMapper SiteFolderPermissionMapper = Create<SiteFolderPermissionMapper>(true);

        internal static readonly EntityTypePermissionMapper EntityTypePermissionMapper = Create<EntityTypePermissionMapper>(true);

        internal static readonly BackendActionPermissionMapper BackendActionPermissionMapper = Create<BackendActionPermissionMapper>(true);

        internal static readonly FieldListItemRowMapper FieldListItemRowMapper = Create<FieldListItemRowMapper>();

        internal static readonly SiteListItemRowMapper SiteListItemRowMapper = Create<SiteListItemRowMapper>();

        internal static readonly ChildEntityPermissionListItemRowMapper ChildEntityPermissionListItemRowMapper = Create<ChildEntityPermissionListItemRowMapper>();

        internal static readonly ActionPermissionTreeNodeRowMapper ActionPermissionTreeNodeRowMapper = Create<ActionPermissionTreeNodeRowMapper>();

        internal static readonly VisualEditorCommandRowMapper VisualEditorCommandRowMapper = Create<VisualEditorCommandRowMapper>();

        internal static readonly PageTemplateRowMapper PageTemplateRowMapper = Create<PageTemplateRowMapper>(true);

        internal static readonly PageTemplateSearchResultRowMapper PageTemplateSearchResultRowMapper = Create<PageTemplateSearchResultRowMapper>(true);

        internal static readonly PageRowMapper PageRowMapper = Create<PageRowMapper>(true);

        internal static readonly ObjectRowMapper ObjectRowMapper = Create<ObjectRowMapper>(true);

        internal static readonly VisualEditorStyleRowMapper VisualEditorStyleRowMapper = Create<VisualEditorStyleRowMapper>();

        internal static readonly VisualEditorStyleListItemRowMapper VisualEditorStyleListItemRowMapper = Create<VisualEditorStyleListItemRowMapper>();

        internal static readonly VisualEditorStyleMapper VisualEditorStyleMapper = Create<VisualEditorStyleMapper>(true);

        internal static readonly StatusTypeListItemRowMapper StatusTypeListItemRowMapper = Create<StatusTypeListItemRowMapper>();

        internal static readonly WorkFlowRuleMapper WorkFlowRuleMapper = Create<WorkFlowRuleMapper>(true);

        internal static readonly NetLanguageMapper NetLanguageMapper = Create<NetLanguageMapper>(true);

        internal static readonly LocaleMapper LocaleMapper = Create<LocaleMapper>(true);

        internal static readonly CharsetMapper CharsetMapper = Create<CharsetMapper>(true);

        internal static readonly PageTemplateMappper PageTemplateMappper = Create<PageTemplateMappper>(true);

        internal static readonly PageMapper PageMapper = Create<PageMapper>(true);

        internal static readonly ObjectMapper ObjectMapper = Create<ObjectMapper>(true);

        internal static readonly ObjectTypeMapper ObjectTypeMapper = Create<ObjectTypeMapper>(true);

        internal static readonly ObjectFormatMapper ObjectFormatMapper = Create<ObjectFormatMapper>(true);

        internal static readonly ObjectFormatRowMapper ObjectFormatRowMapper = Create<ObjectFormatRowMapper>();

        internal static readonly ContainerMapper ContainerMapper = Create<ContainerMapper>(true);

        internal static readonly ContentFormMapper ContentFormMapper = Create<ContentFormMapper>(true);

        internal static readonly ObjectValueMapper ObjectValueMapper = Create<ObjectValueMapper>();

        internal static readonly ArticleListItemRowMapper ArticleListItemRowMapper = Create<ArticleListItemRowMapper>();

        internal static readonly StatusHistoryItemMapper StatusHistoryItemMapper = Create<StatusHistoryItemMapper>(true);

        internal static readonly StatusTypeRowMapper StatusTypeRowMapper = Create<StatusTypeRowMapper>(true);

        internal static readonly ObjectFormatSearchResultRowMapper ObjectFormatSearchResultRowMapper = Create<ObjectFormatSearchResultRowMapper>();

        internal static readonly ObjectSearchResultRowMapper ObjectSearchResultRowMapper = Create<ObjectSearchResultRowMapper>();

        internal static readonly ObjectFormatVersionRowMapper ObjectFormatVersionRowMapper = Create<ObjectFormatVersionRowMapper>();

        internal static readonly ObjectFormatVersionMapper ObjectFormatVersionMapper = Create<ObjectFormatVersionMapper>();

        internal static readonly DbMapper DbMapper = Create<DbMapper>(true);

        internal static readonly TemplateObjectFormatDtoRowMapper TemplateObjectFormatDtoRowMapper = Create<TemplateObjectFormatDtoRowMapper>();

        internal static readonly ExternalNotificationMapper ExternalNotificationMapper = Create<ExternalNotificationMapper>();

        internal static readonly DataRowMapper DataRowMapper = new DataRowMapper();
    }
}
