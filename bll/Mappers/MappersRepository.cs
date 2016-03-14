using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using System.Data;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.BLL.Mappers.EntityPermissions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Repository;

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
            T item = new T();
            item.CreateBizMapper();
            if (withDal)
                item.CreateDalMapper();
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

        internal static readonly SiteMapper SiteMapper = MappersRepository.Create<SiteMapper>(true);

        internal static readonly UserMapper UserMapper = MappersRepository.Create<UserMapper>(true);

        internal static readonly ContentMapper ContentMapper = MappersRepository.Create<ContentMapper>(true);

        internal static readonly ToolbarButtonMapper ToolbarButtonMapper = MappersRepository.Create<ToolbarButtonMapper>(true);

        internal static readonly BackendActionMapper BackendActionMapper = MappersRepository.Create<BackendActionMapper>(true);

        internal static readonly BackendActionStatusMapper BackendActionStatusMapper = MappersRepository.Create<BackendActionStatusMapper>();

        internal static readonly BackendActionTypeMapper BackendActionTypeMapper = MappersRepository.Create<BackendActionTypeMapper>();

		internal static readonly ContextMenuItemRowMapper ContextMenuItemRowMapper = MappersRepository.Create<ContextMenuItemRowMapper>();

		internal static readonly ContextMenuRowMapper ContextMenuRowMapper = MappersRepository.Create<ContextMenuRowMapper>();

        internal static readonly EntityTypeMapper EntityTypeMapper = MappersRepository.Create<EntityTypeMapper>();

		internal static readonly ViewTypeMapper ViewTypeMapper = MappersRepository.Create<ViewTypeMapper>();

		internal static readonly BackendActionViewMapper BackendActionViewMapper = MappersRepository.Create<BackendActionViewMapper>();

        internal static readonly TreeNodeMapper TreeNodeMapper = MappersRepository.Create<TreeNodeMapper>();

        internal static readonly FieldMapper FieldMapper = MappersRepository.Create<FieldMapper>(true);

        internal static readonly FieldTypeMapper FieldTypeMapper = MappersRepository.Create<FieldTypeMapper>();

        internal static readonly ArticleMapper ArticleMapper = MappersRepository.Create<ArticleMapper>(true);

		internal static readonly ArticleRowMapper ArticleRowMapper = MappersRepository.Create<ArticleRowMapper>(true);

        internal static readonly ArticleScheduleMapper ArticleScheduleMapper = MappersRepository.Create<ArticleScheduleMapper>(true);

        internal static readonly StatusMapper StatusTypeMapper = MappersRepository.Create<StatusMapper>(true);

        internal static readonly ContentConstraintMapper ContentConstraintMapper = MappersRepository.Create<ContentConstraintMapper>(true);

		internal static readonly NotificationTemplateFormatMapper NotificationTemplateFormatMapper = MappersRepository.Create<NotificationTemplateFormatMapper>(true);

        internal static readonly ContentConstraintRuleMapper ContentConstraintRuleMapper = MappersRepository.Create<ContentConstraintRuleMapper>(true);

        internal static readonly ContentWorkflowBindMapper ContentWorkflowBindMapper = MappersRepository.Create<ContentWorkflowBindMapper>(true);

        internal static readonly ArticleWorkflowBindMapper ArticleWorkflowBindMapper = MappersRepository.Create<ArticleWorkflowBindMapper>();

        internal static readonly ContentLinkMapper ContentLinkMapper = MappersRepository.Create<ContentLinkMapper>(true);

        internal static readonly ArticleVersionMapper ArticleVersionMapper = MappersRepository.Create<ArticleVersionMapper>();

		internal static readonly SiteFolderMapper SiteFolderMapper = MappersRepository.Create<SiteFolderMapper>(true);

		internal static readonly ContentFolderMapper ContentFolderMapper = MappersRepository.Create<ContentFolderMapper>(true);

        internal static readonly ContentGroupMapper ContentGroupMapper = MappersRepository.Create<ContentGroupMapper>(true);

        internal static readonly DynamicImageMapper DynamicImageMapper = MappersRepository.Create<DynamicImageMapper>(true);

		internal static readonly ArticleScheduleTaskMapper ArticleScheduleTaskMapper = MappersRepository.Create<ArticleScheduleTaskMapper>();

		internal static readonly WorkflowMapper WorkflowMapper = MappersRepository.Create<WorkflowMapper>(true);

		internal static readonly MaskTemplateMapper MaskTemplateMapper = MappersRepository.Create<MaskTemplateMapper>();

		internal static readonly VirtualFieldDataMapper VirtualFieldDataMapper = MappersRepository.Create<VirtualFieldDataMapper>();

		internal static readonly UnionAttrMapper UnionAttrMapper = MappersRepository.Create<UnionAttrMapper>(true);

		internal static readonly UserQueryAttrMapper UserQueryAttrMapper = MappersRepository.Create<UserQueryAttrMapper>(true);		

		internal static readonly VirtualFieldsRelationMapper VirtualFieldsRelationMapper = MappersRepository.Create<VirtualFieldsRelationMapper>();

		internal static readonly UnionFieldRelationCountMapper UnionFieldRelationCountMapper = MappersRepository.Create<UnionFieldRelationCountMapper>();

		internal static readonly BackendActionLogMapper BackendActionLogMapper = MappersRepository.Create<BackendActionLogMapper>(true);

		internal static readonly ButtonTraceRowMapper ButtonTraceRowMapper = MappersRepository.Create<ButtonTraceRowMapper>();

		internal static readonly RemovedEntitiesRowMapper RemovedEntitiesRowMapper = MappersRepository.Create<RemovedEntitiesRowMapper>();		

		internal static readonly BackendActionLogRowMapper BackendActionLogRowMapper = MappersRepository.Create<BackendActionLogRowMapper>();

		internal static readonly ToolbarButtonRowMapper ToolbarButtonRowMapper = MappersRepository.Create<ToolbarButtonRowMapper>();

		internal static readonly SessionsLogMapper SessionsLogMapper = MappersRepository.Create<SessionsLogMapper>(true);

		internal static readonly SessionsLogRowMapper SessionsLogRowMapper = MappersRepository.Create<SessionsLogRowMapper>();

		internal static readonly CustomActionMapper CustomActionMapper = MappersRepository.Create<CustomActionMapper>(true);

		internal static readonly CustomActionListItemRowMapper CustomActionListItemRowMapper = MappersRepository.Create<CustomActionListItemRowMapper>();

		internal static readonly ContextMenuMapper ContextMenuMapper = MappersRepository.Create<ContextMenuMapper>();

		internal static readonly ContextMenuItemMapper ContextMenuItemMapper = MappersRepository.Create<ContextMenuItemMapper>(true);

        internal static readonly ContentListItemRowMapper ContentListItemRowMapper = MappersRepository.Create<ContentListItemRowMapper>();
		
		internal static readonly UserListItemRowMapper UserListItemRowMapper = MappersRepository.Create<UserListItemRowMapper>();

		internal static readonly UserGroupMapper UserGroupMapper = MappersRepository.Create<UserGroupMapper>(true);

		internal static readonly UserGroupListItemRowMapper UserGroupListItemRowMapper = MappersRepository.Create<UserGroupListItemRowMapper>();

		internal static readonly NotificationMapper NotificationMapper = MappersRepository.Create<NotificationMapper>(true);		

        internal static readonly NotificationListItemRowMapper NotificationListItemRowMapper = MappersRepository.Create<NotificationListItemRowMapper>();

		internal static readonly VisualEditorPluginListItemRowMapper VisualEditorPluginListItemRowMapper = MappersRepository.Create<VisualEditorPluginListItemRowMapper>();

		internal static readonly WorkflowListItemRowMapper WorkflowListItemRowMapper = MappersRepository.Create<WorkflowListItemRowMapper>();

		internal static readonly VisualEditorPluginMapper VisualEditorPluginMapper = MappersRepository.Create<VisualEditorPluginMapper>(true);

		internal static readonly VisualEditorCommandMapper VisualEditorCommandMapper = MappersRepository.Create<VisualEditorCommandMapper>(true);

		internal static readonly EntityPermissionLevelMapper EntityPermissionLevelMapper = MappersRepository.Create<EntityPermissionLevelMapper>();

		internal static readonly PermissionListItemRowMapper PermissionListItemRowMapper = MappersRepository.Create<PermissionListItemRowMapper>();

		internal static readonly SitePermissionMapper SitePermissionMapper = MappersRepository.Create<SitePermissionMapper>(true);

		internal static readonly ContentPermissionMapper ContentPermissionMapper = MappersRepository.Create<ContentPermissionMapper>(true);

		internal static readonly ArticlePermissionMapper ArticlePermissionMapper = MappersRepository.Create<ArticlePermissionMapper>(true);

		internal static readonly WorkflowPermissionMapper WorkflowPermissionMapper = MappersRepository.Create<WorkflowPermissionMapper>(true);

		internal static readonly SiteFolderPermissionMapper SiteFolderPermissionMapper = MappersRepository.Create<SiteFolderPermissionMapper>(true);

		internal static readonly EntityTypePermissionMapper EntityTypePermissionMapper = MappersRepository.Create<EntityTypePermissionMapper>(true);

		internal static readonly BackendActionPermissionMapper BackendActionPermissionMapper = MappersRepository.Create<BackendActionPermissionMapper>(true);

        internal static readonly FieldListItemRowMapper FieldListItemRowMapper = MappersRepository.Create<FieldListItemRowMapper>();

        internal static readonly SiteListItemRowMapper SiteListItemRowMapper = MappersRepository.Create<SiteListItemRowMapper>();

		internal static readonly ChildEntityPermissionListItemRowMapper ChildEntityPermissionListItemRowMapper = MappersRepository.Create<ChildEntityPermissionListItemRowMapper>();

		internal static readonly ActionPermissionTreeNodeRowMapper ActionPermissionTreeNodeRowMapper = MappersRepository.Create<ActionPermissionTreeNodeRowMapper>();

		internal static readonly VisualEditorCommandRowMapper VisualEditorCommandRowMapper = MappersRepository.Create<VisualEditorCommandRowMapper>();		

        internal static readonly PageTemplateRowMapper PageTemplateRowMapper = MappersRepository.Create<PageTemplateRowMapper>(true);

		internal static readonly PageTemplateSearchResultRowMapper PageTemplateSearchResultRowMapper = MappersRepository.Create<PageTemplateSearchResultRowMapper>(true);

        internal static readonly PageRowMapper PageRowMapper = MappersRepository.Create<PageRowMapper>(true);

        internal static readonly ObjectRowMapper ObjectRowMapper = MappersRepository.Create<ObjectRowMapper>(true);

		internal static readonly VisualEditorStyleRowMapper VisualEditorStyleRowMapper = MappersRepository.Create<VisualEditorStyleRowMapper>();

		internal static readonly VisualEditorStyleListItemRowMapper VisualEditorStyleListItemRowMapper = MappersRepository.Create<VisualEditorStyleListItemRowMapper>();

		internal static readonly VisualEditorStyleMapper VisualEditorStyleMapper = MappersRepository.Create<VisualEditorStyleMapper>(true);

		internal static readonly StatusTypeListItemRowMapper StatusTypeListItemRowMapper = MappersRepository.Create<StatusTypeListItemRowMapper>();

		internal static readonly WorkFlowRuleMapper WorkFlowRuleMapper = MappersRepository.Create<WorkFlowRuleMapper>(true);

        internal static readonly NetLanguageMapper NetLanguageMapper = MappersRepository.Create<NetLanguageMapper>(true);

        internal static readonly LocaleMapper LocaleMapper = MappersRepository.Create<LocaleMapper>(true);

        internal static readonly CharsetMapper CharsetMapper = MappersRepository.Create<CharsetMapper>(true);

        internal static readonly PageTemplateMappper PageTemplateMappper = MappersRepository.Create<PageTemplateMappper>(true);

        internal static readonly PageMapper PageMapper = MappersRepository.Create<PageMapper>(true);

		internal static readonly ObjectMapper ObjectMapper = MappersRepository.Create<ObjectMapper>(true);

		internal static readonly ObjectTypeMapper ObjectTypeMapper = MappersRepository.Create<ObjectTypeMapper>(true);

		internal static readonly ObjectFormatMapper ObjectFormatMapper = MappersRepository.Create<ObjectFormatMapper>(true);

		internal static readonly ObjectFormatRowMapper ObjectFormatRowMapper = MappersRepository.Create<ObjectFormatRowMapper>();

		internal static readonly ContainerMapper ContainerMapper = MappersRepository.Create<ContainerMapper>(true);

		internal static readonly ContentFormMapper ContentFormMapper = MappersRepository.Create<ContentFormMapper>(true);

		internal static readonly ObjectValueMapper ObjectValueMapper = MappersRepository.Create<ObjectValueMapper>();

		internal static readonly ArticleListItemRowMapper ArticleListItemRowMapper = MappersRepository.Create<ArticleListItemRowMapper>();

        internal static readonly StatusHistoryItemMapper StatusHistoryItemMapper = MappersRepository.Create<StatusHistoryItemMapper>(true);

		internal static readonly StatusTypeRowMapper StatusTypeRowMapper = MappersRepository.Create<StatusTypeRowMapper>(true);

		internal static readonly ObjectFormatSearchResultRowMapper ObjectFormatSearchResultRowMapper = MappersRepository.Create<ObjectFormatSearchResultRowMapper>();

		internal static readonly ObjectSearchResultRowMapper ObjectSearchResultRowMapper = MappersRepository.Create<ObjectSearchResultRowMapper>();

		internal static readonly ObjectFormatVersionRowMapper ObjectFormatVersionRowMapper = MappersRepository.Create<ObjectFormatVersionRowMapper>();

		internal static readonly ObjectFormatVersionMapper ObjectFormatVersionMapper = MappersRepository.Create<ObjectFormatVersionMapper>();

		internal static readonly DbMapper DbMapper = MappersRepository.Create<DbMapper>(true);

		internal static readonly TemplateObjectFormatDtoRowMapper TemplateObjectFormatDtoRowMapper = MappersRepository.Create<TemplateObjectFormatDtoRowMapper>();

		internal static readonly DataRowMapper DataRowMapper = new DataRowMapper();
	}
}
