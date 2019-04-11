using Microsoft.EntityFrameworkCore;

namespace Quantumart.QP8.DAL
{
    public  class ContentCustomActionBindDAL
    {

        public decimal CustomActionId { get; set; }

        public decimal ContentId { get; set; }


        public ContentDAL Content { get; set; }

        public CustomActionDAL CustomAction { get; set; }


    }

    public partial class ContentCustomActionConfiguration : IEntityTypeConfiguration<ContentCustomActionBindDAL>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ContentCustomActionBindDAL> builder)
        {

            builder.ToTable("ACTION_CONTENT_BIND");

            // key
            builder.HasKey(t => new { t.CustomActionId, t.ContentId });

            // properties
            builder.Property(t => t.CustomActionId).HasColumnName("CUSTOM_ACTION_ID");

            builder.Property(t => t.ContentId).HasColumnName("CONTENT_ID");

            // relationships
            builder.HasOne(t => t.Content)
                .WithMany(t => t.ContentCustomActionBinds)
                .HasForeignKey(d => d.ContentId);

            builder.HasOne(t => t.CustomAction)
                .WithMany(t => t.ContentCustomActionBinds)
                .HasForeignKey(d => d.CustomActionId);

        }

    }
}
