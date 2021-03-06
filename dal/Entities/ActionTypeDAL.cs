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
    public partial class ActionTypeDAL
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal RequiredPermissionLevelId { get; set; }
        public byte ItemsAffected { get; set; }

        public PermissionLevelDAL PermissionLevel { get; set; }
        public ICollection<BackendActionDAL> BackendAction { get; set; }
    }
        public class ActionTypeDALConfiguration : IEntityTypeConfiguration<ActionTypeDAL>
        {
            public void Configure(EntityTypeBuilder<ActionTypeDAL> builder)
            {
                builder.ToTable("ACTION_TYPE");

                builder.Property(x => x.Id).HasColumnName("ID");
				builder.Property(x => x.Name).HasColumnName("NAME");
				builder.Property(x => x.Code).HasColumnName("CODE");
				builder.Property(x => x.RequiredPermissionLevelId).HasColumnName("REQUIRED_PERMISSION_LEVEL_ID");
				builder.Property(x => x.ItemsAffected).HasColumnName("ITEMS_AFFECTED");


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.PermissionLevel).WithMany(y => y.ActionType).HasForeignKey(x => x.RequiredPermissionLevelId);
    			builder.HasMany(x => x.BackendAction).WithOne(y => y.ActionType).HasForeignKey(y => y.TypeId);

            }
        }
}
