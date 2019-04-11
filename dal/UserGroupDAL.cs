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
    public class UserGroupDAL
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal SharedArticles { get; set; }
        public string NtGroup { get; set; }
        public byte[] AdSid { get; set; }
        public bool BuiltIn { get; set; }
        public bool IsReadOnly { get; set; }
        public bool UseParallelWorkflow { get; set; }
        public bool CanUnlockItems { get; set; }

        public ICollection<ContentPermissionDAL> ContentAccess { get; set; }
        public ICollection<ContentFolderAccessDAL> ContentFolderAccess { get; set; }
        public ICollection<ArticlePermissionDAL> ArticleAccess { get; set; }
        public ICollection<SiteFolderPermissionDAL> FolderAccess { get; set; }
        public ICollection<NotificationsDAL> Notifications { get; set; }
        public ICollection<SitePermissionDAL> SiteAccess { get; set; }
        public ICollection<WorkflowPermissionDAL> WorkflowAccess { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules { get; set; }


        public UserDAL LastModifiedByUser { get; set; }
        public ICollection<BackendActionPermissionDAL> ACTION_ACCESS { get; set; }
        public ICollection<EntityTypePermissionDAL> ENTITY_TYPE_ACCESS { get; set; }
        public ICollection<UserUserGroupBindDAL> UserGroupBinds { get; set; }
        public ICollection<GroupToGroupBindDAL> ParentGroupToGroupBinds { get; set; }
        public ICollection<GroupToGroupBindDAL> ChildGroupToGroupBinds { get; set; }

        [NotMapped]
        public IEnumerable<UserDAL> Users => UserGroupBinds?.Select(x => x.User);

        [NotMapped]
        public IEnumerable<UserGroupDAL> ParentGroups => ParentGroupToGroupBinds?.Select(x => x.ParentGroup);

        [NotMapped]
        public IEnumerable<UserGroupDAL> ChildGroups => ChildGroupToGroupBinds?.Select(x => x.ChildGroup);


    }

    public class UserGroupDALConfiguration : IEntityTypeConfiguration<UserGroupDAL>
    {
        public void Configure(EntityTypeBuilder<UserGroupDAL> builder)
        {
            builder.ToTable("USER_GROUP");

            builder.Property(x => x.CanUnlockItems).HasColumnName("CAN_UNLOCK_ITEMS");
            builder.Property(x => x.Id).HasColumnName("GROUP_ID");
            builder.Property(x => x.Name).HasColumnName("GROUP_NAME");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.SharedArticles).HasColumnName("shared_content_items");
            builder.Property(x => x.NtGroup).HasColumnName("nt_group");
            builder.Property(x => x.AdSid).HasColumnName("ad_sid");
            builder.Property(x => x.BuiltIn).HasColumnName("BUILT_IN");
            builder.Property(x => x.IsReadOnly).HasColumnName("READONLY");
            builder.Property(x => x.UseParallelWorkflow).HasColumnName("use_parallel_workflow");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.ContentAccess).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.ContentFolderAccess).WithOne(y => y.UserGroup).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.ArticleAccess).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.FolderAccess).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.Notifications).WithOne(y => y.ToUserGroup).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.SiteAccess).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.WorkflowAccess).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.WorkflowRules).WithOne(y => y.UserGroup).HasForeignKey(y => y.GroupId);


            builder.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedBy);
            builder.HasMany(x => x.ACTION_ACCESS).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);
            builder.HasMany(x => x.ENTITY_TYPE_ACCESS).WithOne(y => y.Group).HasForeignKey(y => y.GroupId);


            builder.HasMany(x => x.UserGroupBinds).WithOne(y => y.UserGroup).HasForeignKey(y => y.UserGroupId);
            builder.HasMany(x => x.ChildGroupToGroupBinds).WithOne(y => y.ChildGroup).HasForeignKey(y => y.ChildGroupId);
            builder.HasMany(x => x.ParentGroupToGroupBinds).WithOne(y => y.ParentGroup).HasForeignKey(y => y.ParentGroupId);

        }
    }
}
