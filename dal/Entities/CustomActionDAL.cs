using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class CustomActionDAL : IQpEntityObject
    {
        public decimal Id { get; set; }
        public int ActionId { get; set; }
        public string Alias { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public int Order { get; set; }
        public bool SiteExcluded { get; set; }
        public bool ContentExcluded { get; set; }
        public bool ShowInMenu { get; set; }
        public bool ShowInToolbar { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public BackendActionDAL Action { get; set; }


        public UserDAL LastModifiedByUser { get; set; }

        public ICollection<ContentCustomActionBindDAL> ContentCustomActionBinds { get; set; } = new HashSet<ContentCustomActionBindDAL>();
        public ICollection<SiteCustomActionBindDAL> SiteCustomActionBinds { get; set; } = new HashSet<SiteCustomActionBindDAL>();

        [NotMapped]
        public IEnumerable<ContentDAL> Contents => ContentCustomActionBinds?.Select(x => x.Content);

        [NotMapped]
        public IEnumerable<SiteDAL> Sites => SiteCustomActionBinds?.Select(x => x.Site);
    }

    public class CustomActionDALConfiguration : IEntityTypeConfiguration<CustomActionDAL>
    {
        public void Configure(EntityTypeBuilder<CustomActionDAL> builder)
        {
            builder.ToTable("CUSTOM_ACTION");

            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.Name).HasColumnName("NAME");
            builder.Property(x => x.Alias).HasColumnName("ALIAS");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.ShowInToolbar).HasColumnName("SHOW_IN_TOOLBAR");
            builder.Property(x => x.ShowInMenu).HasColumnName("SHOW_IN_MENU");
            builder.Property(x => x.ContentExcluded).HasColumnName("CONTENT_EXCLUDED");
            builder.Property(x => x.SiteExcluded).HasColumnName("SITE_EXCLUDED");
            builder.Property(x => x.Order).HasColumnName("ORDER");
            builder.Property(x => x.IconUrl).HasColumnName("ICON_URL");
            builder.Property(x => x.Url).HasColumnName("URL");
            builder.Property(x => x.ActionId).HasColumnName("ACTION_ID");
            builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Action).WithMany(y => y.CustomActions).HasForeignKey(x => x.ActionId);
            builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.CUSTOM_ACTION).HasForeignKey(x => x.LastModifiedBy);

            builder.HasMany(x => x.ContentCustomActionBinds).WithOne(y => y.CustomAction).HasForeignKey(y => y.CustomActionId);
            builder.HasMany(x => x.SiteCustomActionBinds).WithOne(y => y.CustomAction).HasForeignKey(y => y.CustomActionId);
            // builder.HasMany(x => x.Sites).WithMany(y => y.CustomActions).HasForeignKey(y => y.);

        }
    }
}
