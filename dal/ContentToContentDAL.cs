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
    public partial class ContentToContentDAL
    {
    
        public decimal LinkId { get; set; }
        public decimal LContentId { get; set; }
        public decimal RContentId { get; set; }
        public bool MapAsClass { get; set; }
        public string NetLinkName { get; set; }
        public string NetPluralLinkName { get; set; }
        public bool Symmetric { get; set; }
    
        public ContentDAL Content { get; set; }
        public ContentDAL Content1 { get; set; }
        public ICollection<FieldDAL> Field { get; set; }
        public ICollection<ItemToItemDAL> ItemToItem { get; set; }
    }
        public class ContentToContentDALConfiguration : IEntityTypeConfiguration<ContentToContentDAL>
        {
            public void Configure(EntityTypeBuilder<ContentToContentDAL> builder)
            {
                builder.ToTable("content_to_content");
    
                builder.Property(x => x.Symmetric).HasColumnName("SYMMETRIC");
				builder.Property(x => x.LinkId).HasColumnName("link_id");
				builder.Property(x => x.LContentId).HasColumnName("l_content_id");
				builder.Property(x => x.RContentId).HasColumnName("r_content_id");
				builder.Property(x => x.MapAsClass).HasColumnName("MAP_AS_CLASS");
				builder.Property(x => x.NetLinkName).HasColumnName("NET_LINK_NAME");
				builder.Property(x => x.NetPluralLinkName).HasColumnName("NET_PLURAL_LINK_NAME");
				
    
                builder.HasKey(x => x.LinkId);
    
                builder.HasOne(x => x.Content).WithMany(y => y.LinkedContents).HasForeignKey(x => x.LContentId);
    			builder.HasOne(x => x.Content1).WithMany(y => y.BackLinkedContents).HasForeignKey(x => x.RContentId);
    			builder.HasMany(x => x.Field).WithOne(y => y.ContentToContent).HasForeignKey(y => y.LinkId);
    			builder.HasMany(x => x.ItemToItem).WithOne(y => y.ContentToContent).HasForeignKey(y => y.LinkId);
    			
            }
        }
}
