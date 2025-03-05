using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities;

public class ExternalWorkflowInProgressDAL
{
    public decimal Id { get; set; }
    public decimal ExternalWorkflowId { get; set; }
    public decimal CurrentStatusId { get; set; }

    public virtual ExternalWorkflowDAL ExternalWorkflow { get; set; }
    public virtual ExternalWorkflowStatusDAL CurrentStatus { get; set; }
}

public class ExternalWorkflowInProgressDALConfiguration : IEntityTypeConfiguration<ExternalWorkflowInProgressDAL>
{
    public void Configure(EntityTypeBuilder<ExternalWorkflowInProgressDAL> builder)
    {
        builder.ToTable("EXTERNAL_WORKFLOW_IN_PROGRESS");

        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.ExternalWorkflowId).HasColumnName("EXTERNAL_WORKFLOW_ID");
        builder.Property(x => x.CurrentStatusId).HasColumnName("CURRENT_STATUS");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.ExternalWorkflow).WithMany(x => x.InProgressWorkflows);
        builder.HasOne(x => x.CurrentStatus).WithMany(x => x.InProgressWorkflows);
    }
}
