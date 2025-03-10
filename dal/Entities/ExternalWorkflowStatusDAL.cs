using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities;

public class ExternalWorkflowStatusDAL
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public string Status { get; set; }
    public decimal ExternalWorkflowId { get; set; }
}

public class ExternalWorkflowHistoryDALConfiguration : IEntityTypeConfiguration<ExternalWorkflowStatusDAL>
{
    public void Configure(EntityTypeBuilder<ExternalWorkflowStatusDAL> builder)
    {
        builder.ToTable("EXTERNAL_WORKFLOW_STATUS");

        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.Created).HasColumnName("CREATED");
        builder.Property(x => x.CreatedBy).HasColumnName("CREATED_BY");
        builder.Property(x => x.Status).HasColumnName("STATUS");
        builder.Property(x => x.ExternalWorkflowId).HasColumnName("EXTERNAL_WORKFLOW_ID");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
