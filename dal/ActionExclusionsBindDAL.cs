using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Quantumart.QP8.DAL
{
    public class ActionExclusionsBindDAL
    {
        public int ExcludedById{ get; set; }
        public int ExcludesId { get; set; }


        public BackendActionDAL ExcludedBy { get; set; }
        public BackendActionDAL Excludes { get; set; }
    }


    public class ActionExclusionsBindDALConfiguration : IEntityTypeConfiguration<ActionExclusionsBindDAL>
    {
        public void Configure(EntityTypeBuilder<ActionExclusionsBindDAL> builder)
        {
            builder.ToTable("ACTION_EXCLUSIONS");

            builder.Property(x => x.ExcludedById).HasColumnName("EXCLUDED_BY_ID");
            builder.Property(x => x.ExcludesId).HasColumnName("EXCLUDES_ID");

            builder.HasKey(x => new { x.ExcludedById, x.ExcludesId });

            builder.HasOne(x => x.Excludes).WithMany(y => y.ExcludesBinds).HasForeignKey(x => x.ExcludesId);
            builder.HasOne(x => x.ExcludedBy).WithMany(y => y.ExcludedByBinds).HasForeignKey(x => x.ExcludedById);


        }
    }
}
