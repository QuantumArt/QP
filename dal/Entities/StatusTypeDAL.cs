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
    public class StatusTypeDAL : IQpEntityObject
    {
        public decimal SiteId { get; set; }
        public decimal Id { get; set; }
        public string Name { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public bool BuiltIn { get; set; }
        public string Color { get; set; }
        public string AltColor { get; set; }

        public ICollection<ArticleDAL> Articles { get; set; }
        public ICollection<NotificationsDAL> Notifications { get; set; }
        public SiteDAL Site { get; set; }
        public ICollection<WorkflowRulesDAL> WorkflowRules { get; set; }
        public ICollection<WaitingForApprovalDAL> WaitingForApproval { get; set; }

        public UserDAL LastModifiedByUser { get; set; }

        public ICollection<ObjectStatusTypeBindDAL> ObjectStatusTypeBinds { get; set; }

        [NotMapped]
        public IEnumerable<ObjectDAL> Objects => ObjectStatusTypeBinds?.Select(x => x.Object);
    }

    public class StatusTypeDALConfiguration : IEntityTypeConfiguration<StatusTypeDAL>
    {
        public void Configure(EntityTypeBuilder<StatusTypeDAL> builder)
        {
            builder.ToTable("STATUS_TYPE");

            builder.Property(x => x.AltColor).HasColumnName("ALT_COLOR");
            builder.Property(x => x.Color).HasColumnName("COLOR");
            builder.Property(x => x.BuiltIn).HasColumnName("BUILT_IN");
            builder.Property(x => x.SiteId).HasColumnName("SITE_ID");
            builder.Property(x => x.Id).HasColumnName("STATUS_TYPE_ID").ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasColumnName("STATUS_TYPE_NAME");
            builder.Property(x => x.Weight).HasColumnName("WEIGHT");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");

            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Articles).WithOne(y => y.Status).HasForeignKey(y => y.StatusTypeId);
            builder.HasMany(x => x.Notifications).WithOne(y => y.OnStatusType).HasForeignKey(y => y.NotifyOnStatusTypeId);
            builder.HasOne(x => x.Site).WithMany(y => y.Statuses).HasForeignKey(x => x.SiteId);
            builder.HasMany(x => x.WorkflowRules).WithOne(y => y.StatusType).HasForeignKey(y => y.SuccessorStatusId);
            builder.HasMany(x => x.WaitingForApproval).WithOne(y => y.StatusType).HasForeignKey(y => y.StatusTypeId);
            builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.STATUS_TYPE).HasForeignKey(x => x.LastModifiedBy);

            builder.HasMany(x => x.ObjectStatusTypeBinds).WithOne(y => y.StatusType).HasForeignKey(y => y.StatusTypeId);
            // builder.HasMany(x => x.Objects).WithMany(y => y.StatusType).HasForeignKey(y => y.);
        }
    }
}
