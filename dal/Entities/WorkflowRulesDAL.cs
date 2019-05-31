//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class WorkflowRulesDAL
    {

        public decimal Id { get; set; }
        public Nullable<decimal> UserId { get; set; }
        public Nullable<decimal> GroupId { get; set; }
        public decimal RuleOrder { get; set; }
        public Nullable<decimal> PredecessorPermissionId { get; set; }
        public Nullable<decimal> SuccessorPermissionId { get; set; }
        public decimal SuccessorStatusId { get; set; }
        public string Description { get; set; }
        public decimal WorkflowId { get; set; }

        public PermissionLevelDAL PermissionLevel { get; set; }
        public StatusTypeDAL StatusType { get; set; }
        public UserGroupDAL UserGroup { get; set; }
        public UserDAL User { get; set; }
        public WorkflowDAL Workflow { get; set; }
    }
        public class WorkflowRulesDALConfiguration : IEntityTypeConfiguration<WorkflowRulesDAL>
        {
            public void Configure(EntityTypeBuilder<WorkflowRulesDAL> builder)
            {
                builder.ToTable("workflow_rules");

                builder.Property(x => x.Id).HasColumnName("WORKFLOW_RULE_ID");
				builder.Property(x => x.UserId).HasColumnName("USER_ID");
				builder.Property(x => x.GroupId).HasColumnName("GROUP_ID");
				builder.Property(x => x.RuleOrder).HasColumnName("RULE_ORDER");
				builder.Property(x => x.PredecessorPermissionId).HasColumnName("PREDECESSOR_PERMISSION_ID");
				builder.Property(x => x.SuccessorPermissionId).HasColumnName("SUCCESSOR_PERMISSION_ID");
				builder.Property(x => x.SuccessorStatusId).HasColumnName("SUCCESSOR_STATUS_ID");
				builder.Property(x => x.Description).HasColumnName("COMMENT");
				builder.Property(x => x.WorkflowId).HasColumnName("WORKFLOW_ID");


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.PermissionLevel).WithMany(y => y.WorkflowRules).HasForeignKey(x => x.PredecessorPermissionId);
    			builder.HasOne(x => x.StatusType).WithMany(y => y.WorkflowRules).HasForeignKey(x => x.SuccessorStatusId);
    			builder.HasOne(x => x.UserGroup).WithMany(y => y.WorkflowRules).HasForeignKey(x => x.GroupId);
    			builder.HasOne(x => x.User).WithMany(y => y.WorkflowRules).HasForeignKey(x => x.UserId);
    			builder.HasOne(x => x.Workflow).WithMany(y => y.WorkflowRules).HasForeignKey(x => x.WorkflowId);

            }
        }
}