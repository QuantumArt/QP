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
    public partial class CodeSnippetDAL
    {

        public decimal SnippetId { get; set; }
        public string SnippetName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public Nullable<decimal> SiteId { get; set; }

        public SiteDAL Site { get; set; }
    }
        public class CodeSnippetDALConfiguration : IEntityTypeConfiguration<CodeSnippetDAL>
        {
            public void Configure(EntityTypeBuilder<CodeSnippetDAL> builder)
            {
                builder.ToTable("CODE_SNIPPET");

                builder.Property(x => x.SnippetId).HasColumnName("SNIPPET_ID");
				builder.Property(x => x.SnippetName).HasColumnName("SNIPPET_NAME");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Code).HasColumnName("CODE");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.SiteId).HasColumnName("SITE_ID");


                builder.HasKey(x => x.SnippetId);

                builder.HasOne(x => x.Site).WithMany(y => y.CodeSnippets).HasForeignKey(x => x.SiteId);

            }
        }
}