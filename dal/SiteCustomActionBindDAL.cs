using Microsoft.EntityFrameworkCore;

namespace Quantumart.QP8.DAL
{
    public  class SiteCustomActionBindDAL
    {

        public decimal CustomActionId { get; set; }

        public decimal SiteId { get; set; }


        public SiteDAL Site { get; set; }

        public CustomActionDAL CustomAction { get; set; }


    }

    public partial class SiteCustomActionConfiguration : IEntityTypeConfiguration<SiteCustomActionBindDAL>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<SiteCustomActionBindDAL> builder)
        {

            builder.ToTable("ACTION_SITE_BIND");

            // key
            builder.HasKey(t => new { t.CustomActionId, t.SiteId });

            // properties
            builder.Property(t => t.CustomActionId).HasColumnName("CUSTOM_ACTION_ID");

            builder.Property(t => t.SiteId).HasColumnName("SITE_ID");

            // relationships
            builder.HasOne(t => t.Site)
                .WithMany(t => t.SiteCustomActionBinds)
                .HasForeignKey(d => d.SiteId);

            builder.HasOne(t => t.CustomAction)
                .WithMany(t => t.SiteCustomActionBinds)
                .HasForeignKey(d => d.CustomActionId);

        }

    }
}
