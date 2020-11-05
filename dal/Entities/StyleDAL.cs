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

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class StyleDAL
    {

        public decimal Id { get; set; }
        public Nullable<decimal> TagId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public Nullable<decimal> SiteId { get; set; }
        public bool Direct { get; set; }

        public SiteDAL Site { get; set; }
        public ICollection<StyleAttributeDAL> StyleAttribute { get; set; }
    }
        public class StyleDALConfiguration : IEntityTypeConfiguration<StyleDAL>
        {
            public void Configure(EntityTypeBuilder<StyleDAL> builder)
            {
                builder.ToTable("STYLE");

                builder.Property(x => x.Id).HasColumnName("STYLE_ID");
				builder.Property(x => x.TagId).HasColumnName("STYLE_TAG_ID");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Name).HasColumnName("STYLE_NAME");
				builder.Property(x => x.Class).HasColumnName("CLASS");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.SiteId).HasColumnName("SITE_ID");
				builder.Property(x => x.Direct).HasColumnName("DIRECT");


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Site).WithMany(y => y.Styles).HasForeignKey(x => x.SiteId);
    			builder.HasMany(x => x.StyleAttribute).WithOne(y => y.Style).HasForeignKey(y => y.StyleId);

            }
        }
}