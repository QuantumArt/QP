using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL
{
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class UserDAL : IQpEntityObject
    {
        public decimal Id { get; set; }
        public decimal Disabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal AutoLogOn { get; set; }
        public string NTLogOn { get; set; }
        public DateTime? LastLogOn { get; set; }
        public decimal Subscribed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal? LanguageId { get; set; }
        public decimal VMode { get; set; }
        public byte[] AdSid { get; set; }
        public decimal AllowStageEditField { get; set; }
        public decimal AllowStageEditObject { get; set; }
        public bool BuiltIn { get; set; }
        public string LogOn { get; set; }
        public DateTime PasswordModified { get; set; }
        public string PASSWORD { get; set; }
        public bool EnableContentGroupingInTree { get; set; }
        public bool MustChangePassword { get; set; }

        public ICollection<ContainerDAL> Container { get; set; }
        public ICollection<ContentDAL> Content { get; set; }
        public ICollection<ContentPermissionDAL> ContentAccess { get; set; }
        public ICollection<FieldDAL> Field { get; set; }
        public ICollection<ContentFolderAccessDAL> ContentFolderAccess { get; set; }
        public ICollection<ContentFormDAL> ContentForm { get; set; }
        public ICollection<ArticleDAL> LockedByArticles { get; set; }
        public ICollection<ArticlePermissionDAL> AccessRules { get; set; }
        public ICollection<SiteFolderPermissionDAL> FolderAccess { get; set; }
        public LanguagesDAL Languages { get; set; }
        public ICollection<NotificationsDAL> NotificationsForBackendUser { get; set; }
        public ICollection<NotificationsDAL> Notifications { get; set; }
        public ICollection<ObjectDAL> Object { get; set; }
        public ICollection<ObjectFormatDAL> ObjectFormat { get; set; }
        public ICollection<PageDAL> Page { get; set; }
        public ICollection<PageTemplateDAL> PageTemplate { get; set; }
        public ICollection<SitePermissionDAL> SiteAccess { get; set; }
        public ICollection<UserToPanelDAL> UserToPanel { get; set; }
        public ICollection<WorkflowPermissionDAL> WorkflowAccess { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules { get; set; }
        public ICollection<WaitingForApprovalDAL> WaitingForApproval { get; set; }
        public ICollection<SiteDAL> LockedSites { get; set; }

        public ICollection<SiteDAL> LastModifiedSites { get; set; }
        public ICollection<CustomActionDAL> CUSTOM_ACTION { get; set; }
        public ICollection<ContentFolderDAL> content_FOLDER { get; set; }
        public ICollection<SiteFolderDAL> FOLDER { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
        public ICollection<BackendActionPermissionDAL> ACTION_ACCESS { get; set; }
        public ICollection<BackendActionPermissionDAL> ACTION_ACCESS_1 { get; set; }
        public ICollection<EntityTypePermissionDAL> ENTITY_TYPE_ACCESS { get; set; }
        public ICollection<EntityTypePermissionDAL> ENTITY_TYPE_ACCESS1 { get; set; }
        public ICollection<VePluginDAL> VE_PLUGIN { get; set; }
        public ICollection<VeCommandDAL> VE_COMMAND { get; set; }
        public ICollection<VeStyleDAL> VE_STYLE { get; set; }
        public ICollection<StatusTypeDAL> STATUS_TYPE { get; set; }
        public ICollection<WorkflowDAL> workflow { get; set; }
        public ICollection<ArticleVersionDAL> CONTENT_ITEM_VERSION1 { get; set; }
        public ICollection<UserDefaultFilterItemDAL> DefaultFilter { get; set; }
        public ICollection<ObjectDAL> OBJECT { get; set; }
        public ICollection<ObjectFormatDAL> OBJECT_FORMAT { get; set; }
        public ICollection<PageDAL> PAGE { get; set; }
        public ICollection<PageTemplateDAL> PAGE_TEMPLATE { get; set; }
        public ICollection<DbDAL> DB { get; set; }
        public ICollection<DbDAL> DB1 { get; set; }
        public ICollection<ObjectFormatVersionDAL> OBJECT_FORMAT_VERSION { get; set; }
        public ICollection<UserUserGroupBindDAL> UserGroupBinds { get; set; } = new HashSet<UserUserGroupBindDAL>();

        [NotMapped]
        public IEnumerable<UserGroupDAL> Groups => UserGroupBinds?.Select(x => x.UserGroup);
    }

    public class UserDALConfiguration : IEntityTypeConfiguration<UserDAL>
    {
        public void Configure(EntityTypeBuilder<UserDAL> builder)
        {
            builder.ToTable("USERS");

            builder.Property(x => x.EnableContentGroupingInTree).HasColumnName("ENABLE_CONTENT_GROUPING_IN_TREE");
            builder.Property(x => x.MustChangePassword).HasColumnName("MUST_CHANGE_PASSWORD");
            builder.Property(x => x.PASSWORD).HasColumnName("PASSWORD");
            builder.Property(x => x.Id).HasColumnName("USER_ID").ValueGeneratedOnAdd();
            builder.Property(x => x.Disabled).HasColumnName("DISABLED");
            builder.Property(x => x.FirstName).HasColumnName("FIRST_NAME");
            builder.Property(x => x.LastName).HasColumnName("LAST_NAME");
            builder.Property(x => x.Email).HasColumnName("EMAIL");
            builder.Property(x => x.AutoLogOn).HasColumnName("AUTO_LOGIN");
            builder.Property(x => x.NTLogOn).HasColumnName("NT_LOGIN");
            builder.Property(x => x.LastLogOn).HasColumnName("LAST_LOGIN");
            builder.Property(x => x.Subscribed).HasColumnName("SUBSCRIBED");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.LanguageId).HasColumnName("LANGUAGE_ID");
            builder.Property(x => x.VMode).HasColumnName("VMODE");
            builder.Property(x => x.AdSid).HasColumnName("ad_sid");
            builder.Property(x => x.AllowStageEditField).HasColumnName("allow_stage_edit_field");
            builder.Property(x => x.AllowStageEditObject).HasColumnName("allow_stage_edit_object");
            builder.Property(x => x.BuiltIn).HasColumnName("BUILT_IN");
            builder.Property(x => x.LogOn).HasColumnName("LOGIN");
            builder.Property(x => x.PasswordModified).HasColumnName("PASSWORD_MODIFIED");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Container).WithOne(y => y.Users).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.Content).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.ContentAccess).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.Field).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.ContentFolderAccess).WithOne(y => y.Users).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.ContentForm).WithOne(y => y.Users).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.LockedByArticles).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.AccessRules).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.FolderAccess).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasOne(x => x.Languages).WithMany(y => y.Users).HasForeignKey(x => x.LanguageId);
            builder.HasMany(x => x.NotificationsForBackendUser).WithOne(y => y.FromUser).HasForeignKey(y => y.FromBackenduserId);
            builder.HasMany(x => x.Notifications).WithOne(y => y.ToUser).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.Object).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.ObjectFormat).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.Page).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.PageTemplate).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);
            builder.HasMany(x => x.SiteAccess).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.UserToPanel).WithOne(y => y.Users).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.WorkflowAccess).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.WorkflowRules).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.WaitingForApproval).WithOne(y => y.Users).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.LockedSites).WithOne(y => y.LockedByUser).HasForeignKey(y => y.LockedBy);

            builder.HasMany(x => x.LastModifiedSites).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.CUSTOM_ACTION).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.content_FOLDER).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.FOLDER).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedBy);
            builder.HasMany(x => x.ACTION_ACCESS).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.ACTION_ACCESS_1).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.ENTITY_TYPE_ACCESS).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.ENTITY_TYPE_ACCESS1).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.VE_PLUGIN).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.VE_COMMAND).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.VE_STYLE).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.STATUS_TYPE).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.workflow).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.CONTENT_ITEM_VERSION1).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.ModifiedBy);
            builder.HasMany(x => x.DefaultFilter).WithOne(y => y.User).HasForeignKey(y => y.UserId);
            builder.HasMany(x => x.OBJECT).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.OBJECT_FORMAT).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.PAGE).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.PAGE_TEMPLATE).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);
            builder.HasMany(x => x.DB).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);

            builder.HasMany(x => x.OBJECT_FORMAT_VERSION).WithOne(y => y.LastModifiedByUser).HasForeignKey(y => y.LastModifiedBy);

            builder.HasMany(x => x.UserGroupBinds).WithOne(y => y.User).HasForeignKey(y => y.UserId);

            builder.HasMany(x => x.DB1).WithOne().HasForeignKey(y => y.SingleUserId);
        }
    }
}
