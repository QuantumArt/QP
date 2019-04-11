//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Quantumart.QP8.DAL
{
    
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class ContentDataDAL
    {
    
        public decimal FieldId { get; set; }
        public decimal ArticleId { get; set; }
        public string Data { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public string BlobData { get; set; }
        public decimal Id { get; set; }
        public bool NotForReplication { get; set; }
        public bool SPLITTED { get; set; }
    
        public FieldDAL Field { get; set; }
        public ArticleDAL Article { get; set; }
    }
        public class ContentDataDALConfiguration : IEntityTypeConfiguration<ContentDataDAL>
        {
            public void Configure(EntityTypeBuilder<ContentDataDAL> builder)
            {
                builder.ToTable("CONTENT_DATA");
    
                builder.Property(x => x.SPLITTED).HasColumnName("SPLITTED");
				builder.Property(x => x.FieldId).HasColumnName("ATTRIBUTE_ID");
				builder.Property(x => x.ArticleId).HasColumnName("CONTENT_ITEM_ID");
				builder.Property(x => x.Data).HasColumnName("DATA");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.BlobData).HasColumnName("BLOB_DATA");
				builder.Property(x => x.Id).HasColumnName("CONTENT_DATA_ID");
				builder.Property(x => x.NotForReplication).HasColumnName("not_for_replication");
				
    
                builder.HasKey(x => new { x.FieldId, x.ArticleId });

    
                builder.HasOne(x => x.Field).WithMany(y => y.ContentData).HasForeignKey(x => x.FieldId);
    			builder.HasOne(x => x.Article).WithMany(y => y.ContentData).HasForeignKey(x => x.ArticleId);
    			
            }
        }
}
