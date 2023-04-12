using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities;

public class ExternalWorkflowInProgressDAL
{
    public decimal Id { get; set; }
    public decimal ProcessId { get; set; }
    public decimal CurrentStatus { get; set; }
    public decimal LastModifiedBy { get; set; }
    public decimal ArticleId { get; set; }
    public decimal WorkflowId { get; set; }
}

public class ExternalWorkflowInProgressDALConfiguration : IEntityTypeConfiguration<ExternalWorkflowInProgressDAL>
{
    public void Configure(EntityTypeBuilder<ExternalWorkflowInProgressDAL> builder)
    {
        builder.ToTable("EXTERNAL_WORKFLOW_IN_PROGRESS");

        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.ProcessId).HasColumnName("PROCESS_ID");
        builder.Property(x => x.CurrentStatus).HasColumnName("CURRENT_STATUS");
        builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
        builder.Property(x => x.ArticleId).HasColumnName("ARTICLE_ID");
        builder.Property(x => x.WorkflowId).HasColumnName("WORKFLOW_ID");

        builder.HasKey(x => x.Id);
    }
}
