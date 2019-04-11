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
    public partial class PageTraceFormatDAL
    {
    
        public decimal TraceFormatId { get; set; }
        public decimal FormatId { get; set; }
        public Nullable<decimal> ParentTraceFormatId { get; set; }
        public decimal TraceId { get; set; }
        public decimal Number { get; set; }
        public decimal Duration { get; set; }
    
        public ObjectFormatDAL ObjectFormat { get; set; }
        public PageTraceDAL PageTrace { get; set; }
        public ICollection<PageTraceFormatDAL> PageTraceFormat1 { get; set; }
        public PageTraceFormatDAL PageTraceFormat2 { get; set; }
        public ICollection<PageTraceFormatValuesDAL> PageTraceFormatValues { get; set; }
    }
        public class PageTraceFormatDALConfiguration : IEntityTypeConfiguration<PageTraceFormatDAL>
        {
            public void Configure(EntityTypeBuilder<PageTraceFormatDAL> builder)
            {
                builder.ToTable("PAGE_TRACE_FORMAT");
    
                builder.Property(x => x.TraceFormatId).HasColumnName("TRACE_FORMAT_ID");
				builder.Property(x => x.FormatId).HasColumnName("FORMAT_ID");
				builder.Property(x => x.ParentTraceFormatId).HasColumnName("PARENT_TRACE_FORMAT_ID");
				builder.Property(x => x.TraceId).HasColumnName("TRACE_ID");
				builder.Property(x => x.Number).HasColumnName("NUMBER");
				builder.Property(x => x.Duration).HasColumnName("DURATION");
				
    
                builder.HasKey(x => x.TraceFormatId);
    
                builder.HasOne(x => x.ObjectFormat).WithMany(y => y.PageTraceFormat).HasForeignKey(x => x.FormatId);
    			builder.HasOne(x => x.PageTrace).WithMany(y => y.PageTraceFormat).HasForeignKey(x => x.TraceId);
    			builder.HasMany(x => x.PageTraceFormat1).WithOne(y => y.PageTraceFormat2).HasForeignKey(y => y.ParentTraceFormatId);
    			builder.HasOne(x => x.PageTraceFormat2).WithMany(y => y.PageTraceFormat1).HasForeignKey(x => x.ParentTraceFormatId);
    			builder.HasMany(x => x.PageTraceFormatValues).WithOne(y => y.PageTraceFormat).HasForeignKey(y => y.TraceFormatId);
    			
            }
        }
}
