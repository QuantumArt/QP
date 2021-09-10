//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class PluginFieldDAL
    {
        public decimal Id { get; set; }
        public decimal PluginId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueType { get; set; }
        public string RelationType { get; set; }
        public int SortOrder { get; set; }
        public PluginDAL Plugin { get; set; }

        public ICollection<PluginFieldValueDAL> Values { get; set; }
    }
        public class PluginFieldDALConfiguration : IEntityTypeConfiguration<PluginFieldDAL>
        {
            public void Configure(EntityTypeBuilder<PluginFieldDAL> builder)
            {
                builder.ToTable("PLUGIN_FIELD");

                builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
				builder.Property(x => x.PluginId).HasColumnName("PLUGIN_ID");
                builder.Property(x => x.Name).HasColumnName("NAME");
                builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
                builder.Property(x => x.ValueType).HasColumnName("VALUE_TYPE");
                builder.Property(x => x.RelationType).HasColumnName("RELATION_TYPE");
                builder.Property(x => x.SortOrder).HasColumnName("ORDER");


                builder.HasKey(x => x.Id);

    			builder.HasOne(x => x.Plugin).WithMany(y => y.Fields).HasForeignKey(x => x.PluginId);

            }
        }
}
