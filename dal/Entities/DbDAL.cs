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
    public partial class DbDAL :  IQpEntityObject
    {

        public decimal Id { get; set; }
        public bool RecordActions { get; set; }
        public Nullable<decimal> SingleUserId { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public bool UseADSyncService { get; set; }
        public bool UseDpc { get; set; }
        public bool UseTokens { get; set; }
        public bool AutoOpenHome { get; set; }
        public bool UseCdc { get; set; }

        public bool UseS3 { get; set; }

        public UserDAL LastModifiedByUser { get; set; }
    }
        public class DbDALConfiguration : IEntityTypeConfiguration<DbDAL>
        {
            public void Configure(EntityTypeBuilder<DbDAL> builder)
            {
                builder.ToTable("DB");

                builder.Property(x => x.UseS3).HasColumnName("USE_S3");
                builder.Property(x => x.UseCdc).HasColumnName("USE_CDC");
				builder.Property(x => x.UseDpc).HasColumnName("USE_DPC");
				builder.Property(x => x.UseTokens).HasColumnName("USE_TOKENS");
				builder.Property(x => x.AutoOpenHome).HasColumnName("AUTO_OPEN_HOME");
				builder.Property(x => x.UseADSyncService).HasColumnName("USE_AD_SYNC_SERVICE");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.SingleUserId).HasColumnName("SINGLE_USER_ID");
				builder.Property(x => x.RecordActions).HasColumnName("RECORD_ACTIONS");
				builder.Property(x => x.Id).HasColumnName("ID");


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.DB).HasForeignKey(x => x.LastModifiedBy);

            }
        }
}
