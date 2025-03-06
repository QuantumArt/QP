using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities;

public class ExternalWorkflowDAL
{
    public decimal Id { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public string ProcessId { get; set; }
    public string ArticleName { get; set; }
    public string WorkflowName { get; set; }
    public decimal ArticleId { get; set; }
    public decimal WorkflowId { get; set; }
}

public class ExternalWorkflowDALConfiguration : IEntityTypeConfiguration<ExternalWorkflowDAL>
{
    public void Configure(EntityTypeBuilder<ExternalWorkflowDAL> builder)
    {
        builder.ToTable("EXTERNAL_WORKFLOW");

        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.Created).HasColumnName("CREATED");
        builder.Property(x => x.CreatedBy).HasColumnName("CREATED_BY");
        builder.Property(x => x.ProcessId).HasColumnName("PROCESS_ID");
        builder.Property(x => x.ArticleName).HasColumnName("ARTICLE_NAME");
        builder.Property(x => x.WorkflowName).HasColumnName("WORKFLOW_NAME");
        builder.Property(x => x.ArticleId).HasColumnName("ARTICLE_ID");
        builder.Property(x => x.WorkflowId).HasColumnName("WORKFLOW_ID");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
