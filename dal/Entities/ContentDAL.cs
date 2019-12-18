using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable ClassNeverInstantiated.Global
    public class ContentDAL : IQpEntityObject
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal SiteId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public string FriendlyPluralName { get; set; }
        public string FriendlySingularName { get; set; }
        public decimal AllowItemsPermission { get; set; }
        public decimal? GroupId { get; set; }
        public string ExternalId { get; set; }
        public decimal VirtualType { get; set; }
        public decimal? JoinId { get; set; }
        public bool IsShared { get; set; }
        public bool AutoArchive { get; set; }
        public byte MaxNumOfStoredVersions { get; set; }
        public bool CreateVersionControlView { get; set; }
        public int PageSize { get; set; }
        public string Query { get; set; }
        public string AltQuery { get; set; }
        public bool MapAsClass { get; set; }
        public string NetName { get; set; }
        public string NetPluralName { get; set; }
        public bool UseDefaultFiltration { get; set; }
        public string AdditionalContextClassName { get; set; }
        public string XamlValidation { get; set; }
        public bool DisableXamlValidation { get; set; }
        public bool DisableChangingActions { get; set; }
        public decimal? ParentContentId { get; set; }
        public bool UseForContext { get; set; }
        public string FormScript { get; set; }
        public bool ForReplication { get; set; }
        public string TraceImportScript { get; set; }

        public ICollection<ContainerDAL> Containers { get; set; }
        public ICollection<ContentWorkflowBindDAL> WorkflowBinding { get; set; }
        public ICollection<ContentPermissionDAL> AccessRules { get; set; }
        public ICollection<FieldDAL> Fields { get; set; }
        public ICollection<ContentConstraintDAL> Constraints { get; set; }
        public ContentGroupDAL Group { get; set; }
        public ICollection<ContentFolderDAL> Folders { get; set; }
        public ICollection<ContentFormDAL> Forms { get; set; }
        public ICollection<ArticleDAL> Articles { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
        public SiteDAL Site { get; set; }
        public ICollection<ContentToContentDAL> LinkedContents { get; set; }
        public ICollection<ContentToContentDAL> BackLinkedContents { get; set; }
        public ICollection<ContentDAL> SourceContent { get; set; }
        public ContentDAL JoinContents { get; set; }
        public ICollection<NotificationsDAL> Notifications { get; set; }
        public ICollection<UserQueryContentsDAL> UserQueryContents { get; set; }
        public ICollection<UnionContentsDAL> UnionContents { get; set; }
        public ICollection<UnionContentsDAL> UnionContents1 { get; set; }
        public ICollection<UnionContentsDAL> UnionContents2 { get; set; }
        public ICollection<UserQueryAttrsDAL> UserQueryAttrs { get; set; }
        public ICollection<UserQueryContentsDAL> UserQueryContents1 { get; set; }

        public ICollection<UserDefaultFilterItemDAL> USER_DEFAULT_FILTER { get; set; }
        public ContentDAL ParentContent { get; set; }
        public ICollection<ContentDAL> ChildContents { get; set; }
        public ICollection<ContentCustomActionBindDAL> ContentCustomActionBinds { get; set; }

        [NotMapped]
        public IEnumerable<CustomActionDAL> CustomActions => ContentCustomActionBinds?.Select(x => x.CustomAction);
    }

    public class ContentDALConfiguration : IEntityTypeConfiguration<ContentDAL>
    {
        public void Configure(EntityTypeBuilder<ContentDAL> builder)
        {
            builder.ToTable("CONTENT");

            builder.Property(x => x.ForReplication).HasColumnName("FOR_REPLICATION");
            builder.Property(x => x.FormScript).HasColumnName("FORM_SCRIPT");
            builder.Property(x => x.UseForContext).HasColumnName("USE_FOR_CONTEXT");
            builder.Property(x => x.ParentContentId).HasColumnName("PARENT_CONTENT_ID");
            builder.Property(x => x.DisableChangingActions).HasColumnName("DISABLE_CHANGING_ACTIONS");
            builder.Property(x => x.DisableXamlValidation).HasColumnName("DISABLE_XAML_VALIDATION");
            builder.Property(x => x.XamlValidation).HasColumnName("XAML_VALIDATION");
            builder.Property(x => x.Id).HasColumnName("CONTENT_ID").ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasColumnName("CONTENT_NAME");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.SiteId).HasColumnName("SITE_ID");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.FriendlyPluralName).HasColumnName("friendly_name_plural");
            builder.Property(x => x.FriendlySingularName).HasColumnName("friendly_name_singular");
            builder.Property(x => x.AllowItemsPermission).HasColumnName("allow_items_permission");
            builder.Property(x => x.GroupId).HasColumnName("content_group_id");
            builder.Property(x => x.ExternalId).HasColumnName("external_id");
            builder.Property(x => x.VirtualType).HasColumnName("virtual_type");
            builder.Property(x => x.JoinId).HasColumnName("virtual_join_primary_content_id");
            builder.Property(x => x.IsShared).HasColumnName("is_shared");
            builder.Property(x => x.AutoArchive).HasColumnName("AUTO_ARCHIVE");
            builder.Property(x => x.MaxNumOfStoredVersions).HasColumnName("max_num_of_stored_versions");
            builder.Property(x => x.CreateVersionControlView).HasColumnName("version_control_view");
            builder.Property(x => x.PageSize).HasColumnName("content_page_size");
            builder.Property(x => x.Query).HasColumnName("query");
            builder.Property(x => x.AltQuery).HasColumnName("alt_query");
            builder.Property(x => x.MapAsClass).HasColumnName("MAP_AS_CLASS");
            builder.Property(x => x.NetName).HasColumnName("NET_CONTENT_NAME");
            builder.Property(x => x.NetPluralName).HasColumnName("NET_PLURAL_CONTENT_NAME");
            builder.Property(x => x.UseDefaultFiltration).HasColumnName("USE_DEFAULT_FILTRATION");
            builder.Property(x => x.AdditionalContextClassName).HasColumnName("ADD_CONTEXT_CLASS_NAME");
            builder.Property(x => x.TraceImportScript).HasColumnName("TRACE_IMPORT_SCRIPT");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Containers).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.WorkflowBinding).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.AccessRules).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.Fields).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.Constraints).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasOne(x => x.Group).WithMany(y => y.Contents).HasForeignKey(x => x.GroupId);
            builder.HasMany(x => x.Folders).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.Forms).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.Articles).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.Content).HasForeignKey(x => x.LastModifiedBy);
            builder.HasOne(x => x.Site).WithMany(y => y.Contents).HasForeignKey(x => x.SiteId);
            builder.HasMany(x => x.LinkedContents).WithOne(y => y.Content).HasForeignKey(y => y.LContentId);
            builder.HasMany(x => x.BackLinkedContents).WithOne(y => y.Content1).HasForeignKey(y => y.RContentId);
            builder.HasMany(x => x.SourceContent).WithOne(y => y.JoinContents).HasForeignKey(y => y.JoinId);
            builder.HasOne(x => x.JoinContents).WithMany(y => y.SourceContent).HasForeignKey(x => x.JoinId);
            builder.HasMany(x => x.Notifications).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasMany(x => x.UserQueryContents).WithOne(y => y.Content).HasForeignKey(y => y.RealContentId);
            builder.HasMany(x => x.UnionContents).WithOne(y => y.Content).HasForeignKey(y => y.MasterContentId);
            builder.HasMany(x => x.UnionContents1).WithOne(y => y.Content1).HasForeignKey(y => y.UnionContentId);
            builder.HasMany(x => x.UnionContents2).WithOne(y => y.Content2).HasForeignKey(y => y.VirtualContentId);
            builder.HasMany(x => x.UserQueryAttrs).WithOne(y => y.Content).HasForeignKey(y => y.VirtualContentId);
            builder.HasMany(x => x.UserQueryContents1).WithOne(y => y.Content1).HasForeignKey(y => y.VirtualContentId);

            builder.HasMany(x => x.USER_DEFAULT_FILTER).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
            builder.HasOne(x => x.ParentContent).WithMany(y => y.ChildContents).HasForeignKey(x => x.ParentContentId);
            builder.HasMany(x => x.ChildContents).WithOne(y => y.ParentContent).HasForeignKey(y => y.ParentContentId);

            builder.HasMany(x => x.ContentCustomActionBinds).WithOne(y => y.Content).HasForeignKey(y => y.ContentId);
        }
    }
}
