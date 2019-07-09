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
    public partial class WorkflowDAL :  IQpEntityObject
    {

        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal SiteId { get; set; }
        public bool CreateDefaultNotification { get; set; }
        public bool ApplyByDefault { get; set; }

        public ICollection<NotificationsDAL> Notifications { get; set; }
        public SiteDAL Site { get; set; }
        public ICollection<WorkflowPermissionDAL> WorkflowAccess { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
    }
        public class WorkflowDALConfiguration : IEntityTypeConfiguration<WorkflowDAL>
        {
            public void Configure(EntityTypeBuilder<WorkflowDAL> builder)
            {
                builder.ToTable("workflow");

                builder.Property(x => x.Id).HasColumnName("WORKFLOW_ID").ValueGeneratedOnAdd();
				builder.Property(x => x.Name).HasColumnName("WORKFLOW_NAME");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.SiteId).HasColumnName("SITE_ID");
				builder.Property(x => x.CreateDefaultNotification).HasColumnName("create_default_notification");
				builder.Property(x => x.ApplyByDefault).HasColumnName("apply_by_default");


                builder.HasKey(x => x.Id);

                builder.HasMany(x => x.Notifications).WithOne(y => y.Workflow).HasForeignKey(y => y.WorkflowId);
    			builder.HasOne(x => x.Site).WithMany(y => y.Workflows).HasForeignKey(x => x.SiteId);
    			builder.HasMany(x => x.WorkflowAccess).WithOne(y => y.Workflow).HasForeignKey(y => y.WorkflowId);
    			builder.HasMany(x => x.WorkflowRules).WithOne(y => y.Workflow).HasForeignKey(y => y.WorkflowId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.workflow).HasForeignKey(x => x.LastModifiedBy);

            }
        }
}
