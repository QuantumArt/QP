//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class SystemNotificationDAL
    {

        public decimal ID { get; set; }
        public int CdcLastExecutedLsnId { get; set; }
        public string TRANSACTION_LSN { get; set; }
        public System.DateTime TRANSACTION_DATE { get; set; }
        public string URL { get; set; }
        public decimal TRIES { get; set; }
        public string JSON { get; set; }
        public bool Sent { get; set; }
        public System.DateTime CREATED { get; set; }
        public System.DateTime MODIFIED { get; set; }

        public CdcLastExecutedLsn CdcLastExecutedLsn { get; set; }
    }
        public class SystemNotificationDALConfiguration : IEntityTypeConfiguration<SystemNotificationDAL>
        {
            public void Configure(EntityTypeBuilder<SystemNotificationDAL> builder)
            {
                builder.ToTable("SYSTEM_NOTIFICATION_QUEUE");

                builder.Property(x => x.MODIFIED).HasColumnName("MODIFIED");
				builder.Property(x => x.CREATED).HasColumnName("CREATED");
				builder.Property(x => x.Sent).HasColumnName("SENT");
				builder.Property(x => x.JSON).HasColumnName("JSON");
				builder.Property(x => x.TRIES).HasColumnName("TRIES");
				builder.Property(x => x.URL).HasColumnName("URL");
				builder.Property(x => x.TRANSACTION_DATE).HasColumnName("TRANSACTION_DATE");
				builder.Property(x => x.TRANSACTION_LSN).HasColumnName("TRANSACTION_LSN");
				builder.Property(x => x.CdcLastExecutedLsnId).HasColumnName("CdcLastExecutedLsnId");
				builder.Property(x => x.ID).HasColumnName("ID");


                builder.HasKey(x => x.ID);

                builder.HasOne(x => x.CdcLastExecutedLsn).WithMany(y => y.SYSTEM_NOTIFICATION_QUEUE).HasForeignKey(x => x.CdcLastExecutedLsnId);

            }
        }
}