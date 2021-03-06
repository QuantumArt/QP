//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class PermissionLevelDAL
    {

        public decimal Level { get; set; }
        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<ActionTypeDAL> ActionType { get; set; }
        public ICollection<ContainerDAL> Container { get; set; }
        public ICollection<ContainerDAL> Container1 { get; set; }
        public ICollection<ContentFolderAccessDAL> ContentFolderAccess { get; set; }
        public ICollection<SiteFolderPermissionDAL> FolderAccess { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules1 { get; set; }
        public ICollection<ContentPermissionDAL> CONTENT_ACCESS { get; set; }
        public ICollection<ArticlePermissionDAL> CONTENT_ITEM_ACCESS { get; set; }
        public ICollection<SitePermissionDAL> SITE_ACCESS { get; set; }
        public ICollection<WorkflowPermissionDAL> WORKFLOW_ACCESS { get; set; }
        public ICollection<BackendActionPermissionDAL> ACTION_ACCESS { get; set; }
        public ICollection<EntityTypePermissionDAL> ENTITY_TYPE_ACCESS { get; set; }
    }
        public class PermissionLevelDALConfiguration : IEntityTypeConfiguration<PermissionLevelDAL>
        {
            public void Configure(EntityTypeBuilder<PermissionLevelDAL> builder)
            {
                builder.ToTable("PERMISSION_LEVEL");

                builder.Property(x => x.Level).HasColumnName("PERMISSION_LEVEL");
				builder.Property(x => x.Id).HasColumnName("PERMISSION_LEVEL_ID");
				builder.Property(x => x.Name).HasColumnName("PERMISSION_LEVEL_NAME");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");


                builder.HasKey(x => x.Id);

                builder.HasMany(x => x.ActionType).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.RequiredPermissionLevelId);
    			builder.HasMany(x => x.Container).WithOne(y => y.EndPermissionLevel).HasForeignKey(y => y.EndLevel);
    			builder.HasMany(x => x.Container1).WithOne(y => y.StartPermissionLevel).HasForeignKey(y => y.StartLevel);
    			builder.HasMany(x => x.ContentFolderAccess).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.FolderAccess).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.WorkflowRules).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PredecessorPermissionId);
    			builder.HasMany(x => x.WorkflowRules1).WithOne().HasForeignKey(y => y.SuccessorPermissionId);
    			builder.HasMany(x => x.CONTENT_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.CONTENT_ITEM_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.SITE_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.WORKFLOW_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.ACTION_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);
    			builder.HasMany(x => x.ENTITY_TYPE_ACCESS).WithOne(y => y.PermissionLevel).HasForeignKey(y => y.PermissionLevelId);

            }
        }
}
