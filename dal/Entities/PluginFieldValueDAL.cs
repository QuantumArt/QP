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
    public partial class PluginFieldValueDAL
    {
        public decimal Id { get; set; }
        public decimal PluginFieldId { get; set; }
        public string Value { get; set; }
        public decimal? SiteId { get; set; }
        public decimal? ContentAttributeId { get; set; }
        public decimal? ContentId { get; set; }
        public PluginFieldDAL PluginField { get; set; }
        public ContentDAL Content { get; set; }
        public SiteDAL Site { get; set; }
        public FieldDAL ContentAttribute { get; set; }
    }

    public class PluginFieldValueDALConfiguration : IEntityTypeConfiguration<PluginFieldValueDAL>
        {
            public void Configure(EntityTypeBuilder<PluginFieldValueDAL> builder)
            {
                builder.ToTable("PLUGIN_FIELD_VALUE");

                builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
				builder.Property(x => x.PluginFieldId).HasColumnName("PLUGIN_FIELD_ID");
                builder.Property(x => x.Value).HasColumnName("VALUE");
                builder.Property(x => x.ContentId).HasColumnName("CONTENT_ID");//.IsRequired(false);
                builder.Property(x => x.ContentAttributeId).HasColumnName("CONTENT_ATTRIBUTE_ID");//.IsRequired(false);
                builder.Property(x => x.SiteId).HasColumnName("SITE_ID");//.IsRequired(false);


                builder.HasKey(x => x.Id);

    			builder.HasOne(x => x.PluginField).WithMany(y => y.Values).HasForeignKey(x => x.PluginFieldId);
                builder.HasOne(x => x.Content).WithMany(y => y.PluginFieldValues).HasForeignKey(x => x.ContentId);
                builder.HasOne(x => x.Site).WithMany(y => y.PluginFieldValues).HasForeignKey(x => x.SiteId);
                builder.HasOne(x => x.ContentAttribute).WithMany(y => y.PluginFieldValues).HasForeignKey(x => x.ContentAttributeId);

            }
        }
}
