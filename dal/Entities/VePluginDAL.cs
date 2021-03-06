//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class VePluginDAL :  IQpEntityObject
    {

        public decimal Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Order { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }

        public ICollection<VeCommandDAL> VeCommands { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
    }
        public class VePluginDALConfiguration : IEntityTypeConfiguration<VePluginDAL>
        {
            public void Configure(EntityTypeBuilder<VePluginDAL> builder)
            {
                builder.ToTable("VE_PLUGIN");

                builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Order).HasColumnName("ORDER");
				builder.Property(x => x.Url).HasColumnName("URL");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Name).HasColumnName("NAME");
				builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();


                builder.HasKey(x => x.Id);

                builder.HasMany(x => x.VeCommands).WithOne(y => y.VePlugin).HasForeignKey(y => y.PluginId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.VE_PLUGIN).HasForeignKey(x => x.LastModifiedBy);

            }
        }
}
