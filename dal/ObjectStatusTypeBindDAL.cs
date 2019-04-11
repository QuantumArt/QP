using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL
{
    public class ObjectStatusTypeBindDAL
    {
        public decimal ObjectId { get; set; }
        public decimal StatusTypeId { get; set; }

        public ObjectDAL Object { get; set; }
        public StatusTypeDAL StatusType { get; set; }
    }

    public class ObjectStatusTypeBindDALConfiguration : IEntityTypeConfiguration<ObjectStatusTypeBindDAL>
    {
        public void Configure(EntityTypeBuilder<ObjectStatusTypeBindDAL> builder)
        {
            builder.ToTable("CONTAINER_STATUSES");

            builder.Property(x => x.ObjectId).HasColumnName("OBJECT_ID");
            builder.Property(x => x.StatusTypeId).HasColumnName("STATUS_TYPE_ID");

            builder.HasKey(x => new { x.ObjectId, x.StatusTypeId });

            builder
                .HasOne(x => x.Object)
                .WithMany(x => x.ObjectStatusTypeBinds)
                .HasForeignKey(x => x.ObjectId);

            builder
                .HasOne(x => x.StatusType)
                .WithMany(x => x.ObjectStatusTypeBinds)
                .HasForeignKey(x => x.StatusTypeId);
        }
    }
}
