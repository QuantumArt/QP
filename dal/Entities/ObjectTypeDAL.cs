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
    public partial class ObjectTypeDAL
    {

        public decimal Id { get; set; }
        public string Name { get; set; }
        public string RelatedTable { get; set; }
        public string ImageName { get; set; }
        public string Icon { get; set; }

        public ICollection<ObjectDAL> Object { get; set; }
    }
        public class ObjectTypeDALConfiguration : IEntityTypeConfiguration<ObjectTypeDAL>
        {
            public void Configure(EntityTypeBuilder<ObjectTypeDAL> builder)
            {
                builder.ToTable("OBJECT_TYPE");

                builder.Property(x => x.Id).HasColumnName("OBJECT_TYPE_ID");
				builder.Property(x => x.Name).HasColumnName("TYPE_NAME");
				builder.Property(x => x.RelatedTable).HasColumnName("RELATED_TABLE");
				builder.Property(x => x.ImageName).HasColumnName("image_name");
				builder.Property(x => x.Icon).HasColumnName("ICON");


                builder.HasKey(x => x.Id);

                builder.HasMany(x => x.Object).WithOne(y => y.ObjectType).HasForeignKey(y => y.TypeId);

            }
        }
}
