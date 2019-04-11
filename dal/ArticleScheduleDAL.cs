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
    public partial class ArticleScheduleDAL
    {
    
        public decimal ArticleId { get; set; }
        public decimal Id { get; set; }
        public Nullable<decimal> MaximumOccurences { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public int FreqType { get; set; }
        public int FreqInterval { get; set; }
        public int FreqSubdayType { get; set; }
        public int FreqSubdayInterval { get; set; }
        public int FreqRelativeInterval { get; set; }
        public int FreqRecurrenceFactor { get; set; }
        public Nullable<int> ActiveStartDate { get; set; }
        public int ActiveEndDate { get; set; }
        public int ActiveStartTime { get; set; }
        public int ActiveEndTime { get; set; }
        public int Occurences { get; set; }
        public decimal UseDuration { get; set; }
        public decimal Duration { get; set; }
        public string DurationUnits { get; set; }
        public bool Deactivate { get; set; }
        public bool DeleteJob { get; set; }
        public bool UseService { get; set; }
    
        public ArticleDAL Article { get; set; }
    }
        public class ArticleScheduleDALConfiguration : IEntityTypeConfiguration<ArticleScheduleDAL>
        {
            public void Configure(EntityTypeBuilder<ArticleScheduleDAL> builder)
            {
                builder.ToTable("CONTENT_ITEM_SCHEDULE");
    
                builder.Property(x => x.ArticleId).HasColumnName("CONTENT_ITEM_ID");
				builder.Property(x => x.Id).HasColumnName("SCHEDULE_ID");
				builder.Property(x => x.MaximumOccurences).HasColumnName("MAXIMUM_OCCURENCES");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.FreqType).HasColumnName("freq_type");
				builder.Property(x => x.FreqInterval).HasColumnName("freq_interval");
				builder.Property(x => x.FreqSubdayType).HasColumnName("freq_subday_type");
				builder.Property(x => x.FreqSubdayInterval).HasColumnName("freq_subday_interval");
				builder.Property(x => x.FreqRelativeInterval).HasColumnName("freq_relative_interval");
				builder.Property(x => x.FreqRecurrenceFactor).HasColumnName("freq_recurrence_factor");
				builder.Property(x => x.ActiveStartDate).HasColumnName("active_start_date");
				builder.Property(x => x.ActiveEndDate).HasColumnName("active_end_date");
				builder.Property(x => x.ActiveStartTime).HasColumnName("active_start_time");
				builder.Property(x => x.ActiveEndTime).HasColumnName("active_end_time");
				builder.Property(x => x.Occurences).HasColumnName("occurences");
				builder.Property(x => x.UseDuration).HasColumnName("use_duration");
				builder.Property(x => x.Duration).HasColumnName("duration");
				builder.Property(x => x.DurationUnits).HasColumnName("duration_units");
				builder.Property(x => x.Deactivate).HasColumnName("DEACTIVATE");
				builder.Property(x => x.DeleteJob).HasColumnName("DELETE_JOB");
				builder.Property(x => x.UseService).HasColumnName("USE_SERVICE");
				
    
                builder.HasKey(x => x.Id);
    
                builder.HasOne(x => x.Article).WithMany(y => y.Schedules).HasForeignKey(x => x.ArticleId);
    			
            }
        }
}
