//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class FieldDAL :  IQpEntityObject
    {

        public decimal Id { get; set; }
        public decimal ContentId { get; set; }
        public string Name { get; set; }
        public string FormatMask { get; set; }
        public string InputMask { get; set; }
        public decimal Size { get; set; }
        public string DefaultValue { get; set; }
        public decimal TypeId { get; set; }
        public Nullable<decimal> RelationId { get; set; }
        public decimal IndexFlag { get; set; }
        public string Description { get; set; }
        public System.DateTime Modified { get; set; }
        public System.DateTime Created { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal Order { get; set; }
        public decimal Required { get; set; }
        public decimal PermanentFlag { get; set; }
        public decimal PrimaryFlag { get; set; }
        public string RelationCondition { get; set; }
        public decimal DisplayAsRadioButton { get; set; }
        public bool ViewInList { get; set; }
        public bool ReadonlyFlag { get; set; }
        public decimal AllowStageEdit { get; set; }
        public string Configuration { get; set; }
        public Nullable<decimal> BaseImageId { get; set; }
        public Nullable<decimal> PersistentId { get; set; }
        public Nullable<decimal> JoinId { get; set; }
        public Nullable<decimal> LinkId { get; set; }
        public string DefaultBlobValue { get; set; }
        public bool AutoLoad { get; set; }
        public string FriendlyName { get; set; }
        public bool UseSiteLibrary { get; set; }
        public bool UseArchiveArticles { get; set; }
        public bool AutoExpand { get; set; }
        public Nullable<int> RelationPageSize { get; set; }
        public string Doctype { get; set; }
        public bool FullPage { get; set; }
        public bool RenameMatched { get; set; }
        public string Subfolder { get; set; }
        public bool DisableVersionControl { get; set; }
        public bool MapAsProperty { get; set; }
        public string NetName { get; set; }
        public string NetBackName { get; set; }
        public Nullable<bool> PEnterMode { get; set; }
        public Nullable<bool> UseEnglishQuotes { get; set; }
        public Nullable<decimal> BackRelationId { get; set; }
        public bool IsLong { get; set; }
        public string ExternalCss { get; set; }
        public string RootElementClass { get; set; }
        public bool UseForTree { get; set; }
        public bool AutoCheckChildren { get; set; }
        public bool Aggregated { get; set; }
        public Nullable<decimal> ClassifierId { get; set; }
        public bool IsClassifier { get; set; }
        public bool Changeable { get; set; }
        public bool UseRelationSecurity { get; set; }
        public bool CopyPermissionsToChildren { get; set; }
        public string EnumValues { get; set; }
        public bool ShowAsRadioButtons { get; set; }
        public bool UseForDefaultFiltration { get; set; }
        public Nullable<decimal> OrderFieldId { get; set; }
        public Nullable<decimal> ParentFieldId { get; set; }
        public bool Hide { get; set; }
        public bool Override { get; set; }
        public bool UseForContext { get; set; }
        public bool UseForVariations { get; set; }
        public bool OrderByTitle { get; set; }
        public int FieldTitleCount { get; set; }
        public bool IncludeRelationsInTitle { get; set; }
        public bool UseInChildContentFilter { get; set; }
        public bool OptimizeForHierarchy { get; set; }
        public bool IsLocalization { get; set; }
        public bool UseSeparateReverseViews { get; set; }
        public bool DisableListAutoWrap { get; set; }
        public string TaHighlightType { get; set; }
        public decimal MaxDataListItemCount { get; set; }
        public bool TraceImport { get; set; }
        public bool DenyPastDates { get; set; }

        public FieldTypeDAL Type { get; set; }
        public ContentDAL Content { get; set; }
        public ICollection<FieldDAL> JoinedFields { get; set; }
        public FieldDAL JoinKeyField { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
        public ContentToContentDAL ContentToContent { get; set; }
        public ICollection<FieldDAL> JoinVirtualFields { get; set; }
        public FieldDAL JoinSourceField { get; set; }
        public ICollection<FieldDAL> RelatedFields { get; set; }
        public FieldDAL RelationField { get; set; }
        public ICollection<FieldDAL> Thumbnails { get; set; }
        public FieldDAL BaseImage { get; set; }
        public ICollection<ContentDataDAL> ContentData { get; set; }
        public DynamicImageFieldDAL DynamicImageSettings { get; set; }
        public ICollection<ItemToItemVersionDAL> ItemToItemVersion { get; set; }
        public ICollection<NotificationsDAL> DependentNotifications { get; set; }
        public ICollection<VersionContentDataDAL> VersionContentData { get; set; }
        public ICollection<ContentConstraintRuleDAL> ConstraintRule { get; set; }
        public ICollection<FieldDAL> BackRelations { get; set; }
        public FieldDAL BaseRelation { get; set; }
        public ICollection<FieldDAL> Aggregators { get; set; }
        public FieldDAL Classifier { get; set; }
        public FieldDAL ParentField { get; set; }
        public ICollection<FieldDAL> ChildFields { get; set; }
        public ICollection<FieldDAL> OrderField { get; set; }
        public FieldDAL OrderFieldChildren { get; set; }
    }
        public class FieldDALConfiguration : IEntityTypeConfiguration<FieldDAL>
        {
            public void Configure(EntityTypeBuilder<FieldDAL> builder)
            {
                builder.ToTable("CONTENT_ATTRIBUTE");

                builder.Property(x => x.DisableListAutoWrap).HasColumnName("DISABLE_LIST_AUTO_WRAP");
				builder.Property(x => x.UseSeparateReverseViews).HasColumnName("USE_SEPARATE_REVERSE_VIEWS");
				builder.Property(x => x.TaHighlightType).HasColumnName("TA_HIGHLIGHT_TYPE");
				builder.Property(x => x.IsLocalization).HasColumnName("IS_LOCALIZATION");
				builder.Property(x => x.OptimizeForHierarchy).HasColumnName("OPTIMIZE_FOR_HIERARCHY");
				builder.Property(x => x.UseInChildContentFilter).HasColumnName("USE_IN_CHILD_CONTENT_FILTER");
				builder.Property(x => x.IncludeRelationsInTitle).HasColumnName("INCLUDE_RELATIONS_IN_TITLE");
				builder.Property(x => x.FieldTitleCount).HasColumnName("FIELD_TITLE_COUNT");
				builder.Property(x => x.OrderByTitle).HasColumnName("ORDER_BY_TITLE");
				builder.Property(x => x.UseForVariations).HasColumnName("USE_FOR_VARIATIONS");
				builder.Property(x => x.UseForContext).HasColumnName("USE_FOR_CONTEXT");
				builder.Property(x => x.Override).HasColumnName("OVERRIDE");
				builder.Property(x => x.Hide).HasColumnName("HIDE");
				builder.Property(x => x.ParentFieldId).HasColumnName("PARENT_ATTRIBUTE_ID");
				builder.Property(x => x.OrderFieldId).HasColumnName("TREE_ORDER_FIELD");
				builder.Property(x => x.UseForDefaultFiltration).HasColumnName("USE_FOR_DEFAULT_FILTRATION");
				builder.Property(x => x.ShowAsRadioButtons).HasColumnName("SHOW_AS_RADIO_BUTTON");
				builder.Property(x => x.EnumValues).HasColumnName("ENUM_VALUES");
				builder.Property(x => x.CopyPermissionsToChildren).HasColumnName("COPY_PERMISSIONS_TO_CHILDREN");
				builder.Property(x => x.UseRelationSecurity).HasColumnName("USE_RELATION_SECURITY");
				builder.Property(x => x.Changeable).HasColumnName("CHANGEABLE");
				builder.Property(x => x.IsClassifier).HasColumnName("IS_CLASSIFIER");
				builder.Property(x => x.ClassifierId).HasColumnName("CLASSIFIER_ATTRIBUTE_ID");
				builder.Property(x => x.Aggregated).HasColumnName("AGGREGATED");
				builder.Property(x => x.AutoCheckChildren).HasColumnName("AUTO_CHECK_CHILDREN");
				builder.Property(x => x.UseForTree).HasColumnName("USE_FOR_TREE");
				builder.Property(x => x.RootElementClass).HasColumnName("ROOT_ELEMENT_CLASS");
				builder.Property(x => x.ExternalCss).HasColumnName("EXTERNAL_CSS");
				builder.Property(x => x.IsLong).HasColumnName("IS_LONG");
				builder.Property(x => x.BackRelationId).HasColumnName("BACK_RELATED_ATTRIBUTE_ID");
				builder.Property(x => x.Id).HasColumnName("ATTRIBUTE_ID").ValueGeneratedOnAdd();
				builder.Property(x => x.ContentId).HasColumnName("CONTENT_ID");
				builder.Property(x => x.Name).HasColumnName("ATTRIBUTE_NAME");
				builder.Property(x => x.FormatMask).HasColumnName("FORMAT_MASK");
				builder.Property(x => x.InputMask).HasColumnName("INPUT_MASK");
				builder.Property(x => x.Size).HasColumnName("ATTRIBUTE_SIZE");
				builder.Property(x => x.DefaultValue).HasColumnName("DEFAULT_VALUE");
				builder.Property(x => x.TypeId).HasColumnName("ATTRIBUTE_TYPE_ID");
				builder.Property(x => x.RelationId).HasColumnName("RELATED_ATTRIBUTE_ID");
				builder.Property(x => x.IndexFlag).HasColumnName("INDEX_FLAG");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.Order).HasColumnName("ATTRIBUTE_ORDER");
				builder.Property(x => x.Required).HasColumnName("REQUIRED");
				builder.Property(x => x.PermanentFlag).HasColumnName("PERMANENT_FLAG");
				builder.Property(x => x.PrimaryFlag).HasColumnName("PRIMARY_FLAG");
				builder.Property(x => x.RelationCondition).HasColumnName("RELATION_CONDITION");
				builder.Property(x => x.DisplayAsRadioButton).HasColumnName("display_as_radio_button");
				builder.Property(x => x.ViewInList).HasColumnName("view_in_list");
				builder.Property(x => x.ReadonlyFlag).HasColumnName("READONLY_FLAG");
				builder.Property(x => x.AllowStageEdit).HasColumnName("allow_stage_edit");
				builder.Property(x => x.Configuration).HasColumnName("ATTRIBUTE_CONFIGURATION");
				builder.Property(x => x.BaseImageId).HasColumnName("RELATED_IMAGE_ATTRIBUTE_ID");
				builder.Property(x => x.PersistentId).HasColumnName("persistent_attr_id");
				builder.Property(x => x.JoinId).HasColumnName("join_attr_id");
				builder.Property(x => x.LinkId).HasColumnName("link_id");
				builder.Property(x => x.DefaultBlobValue).HasColumnName("DEFAULT_BLOB_VALUE");
				builder.Property(x => x.AutoLoad).HasColumnName("AUTO_LOAD");
				builder.Property(x => x.FriendlyName).HasColumnName("FRIENDLY_NAME");
				builder.Property(x => x.UseSiteLibrary).HasColumnName("use_site_library");
				builder.Property(x => x.UseArchiveArticles).HasColumnName("use_archive_articles");
				builder.Property(x => x.AutoExpand).HasColumnName("AUTO_EXPAND");
				builder.Property(x => x.RelationPageSize).HasColumnName("relation_page_size");
				builder.Property(x => x.Doctype).HasColumnName("doctype");
				builder.Property(x => x.FullPage).HasColumnName("full_page");
				builder.Property(x => x.RenameMatched).HasColumnName("RENAME_MATCHED");
				builder.Property(x => x.Subfolder).HasColumnName("SUBFOLDER");
				builder.Property(x => x.DisableVersionControl).HasColumnName("DISABLE_VERSION_CONTROL");
				builder.Property(x => x.MapAsProperty).HasColumnName("MAP_AS_PROPERTY");
				builder.Property(x => x.NetName).HasColumnName("NET_ATTRIBUTE_NAME");
				builder.Property(x => x.NetBackName).HasColumnName("NET_BACK_ATTRIBUTE_NAME");
				builder.Property(x => x.PEnterMode).HasColumnName("P_ENTER_MODE");
				builder.Property(x => x.UseEnglishQuotes).HasColumnName("USE_ENGLISH_QUOTES");
				builder.Property(x => x.MaxDataListItemCount).HasColumnName("MAX_DATA_LIST_ITEM_COUNT");
                builder.Property(x => x.TraceImport).HasColumnName("TRACE_IMPORT");
                builder.Property(x => x.DenyPastDates).HasColumnName("DENY_PAST_DATES");

                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Type).WithMany(y => y.Field).HasForeignKey(x => x.TypeId);
    			builder.HasOne(x => x.Content).WithMany(y => y.Fields).HasForeignKey(x => x.ContentId);
    			builder.HasMany(x => x.JoinedFields).WithOne(y => y.JoinKeyField).HasForeignKey(y => y.JoinId);
    			builder.HasOne(x => x.JoinKeyField).WithMany(y => y.JoinedFields).HasForeignKey(x => x.JoinId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.Field).HasForeignKey(x => x.LastModifiedBy);
    			builder.HasOne(x => x.ContentToContent).WithMany(y => y.Field).HasForeignKey(x => x.LinkId);
    			builder.HasMany(x => x.JoinVirtualFields).WithOne(y => y.JoinSourceField).HasForeignKey(y => y.PersistentId);
    			builder.HasOne(x => x.JoinSourceField).WithMany(y => y.JoinVirtualFields).HasForeignKey(x => x.PersistentId);
    			builder.HasMany(x => x.RelatedFields).WithOne(y => y.RelationField).HasForeignKey(y => y.RelationId);
    			builder.HasOne(x => x.RelationField).WithMany(y => y.RelatedFields).HasForeignKey(x => x.RelationId);
    			builder.HasMany(x => x.Thumbnails).WithOne(y => y.BaseImage).HasForeignKey(y => y.BaseImageId);
    			builder.HasOne(x => x.BaseImage).WithMany(y => y.Thumbnails).HasForeignKey(x => x.BaseImageId);
    			builder.HasMany(x => x.ContentData).WithOne(y => y.Field).HasForeignKey(y => y.FieldId);
    			builder.HasOne(x => x.DynamicImageSettings).WithOne(y => y.Field).HasForeignKey<DynamicImageFieldDAL>(y => y.Id);
    			builder.HasMany(x => x.ItemToItemVersion).WithOne(y => y.Field).HasForeignKey(y => y.FieldId);
    			builder.HasMany(x => x.DependentNotifications).WithOne(y => y.EmailField).HasForeignKey(y => y.EmailFieldId);
    			builder.HasMany(x => x.VersionContentData).WithOne(y => y.Field).HasForeignKey(y => y.FieldId);
    			builder.HasMany(x => x.ConstraintRule).WithOne(y => y.Field).HasForeignKey(y => y.FieldId);
    			builder.HasMany(x => x.BackRelations).WithOne(y => y.BaseRelation).HasForeignKey(y => y.BackRelationId);
    			builder.HasOne(x => x.BaseRelation).WithMany(y => y.BackRelations).HasForeignKey(x => x.BackRelationId);
    			builder.HasMany(x => x.Aggregators).WithOne(y => y.Classifier).HasForeignKey(y => y.ClassifierId);
    			builder.HasOne(x => x.Classifier).WithMany(y => y.Aggregators).HasForeignKey(x => x.ClassifierId);
    			builder.HasOne(x => x.ParentField).WithMany(y => y.ChildFields).HasForeignKey(y => y.ParentFieldId);
    			builder.HasMany(x => x.ChildFields).WithOne(y => y.ParentField).HasForeignKey(y => y.ParentFieldId);
    			builder.HasMany(x => x.OrderField).WithOne(y => y.OrderFieldChildren).HasForeignKey(y => y.OrderFieldId);
    			builder.HasOne(x => x.OrderFieldChildren).WithMany(y => y.OrderField).HasForeignKey(x => x.OrderFieldId);

            }
        }
}