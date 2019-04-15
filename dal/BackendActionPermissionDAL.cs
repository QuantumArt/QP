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
    public partial class BackendActionPermissionDAL :  IQpEntityObject
    {

        public decimal Id { get; set; }
        public int ActionId { get; set; }
        public Nullable<decimal> UserId { get; set; }
        public Nullable<decimal> GroupId { get; set; }
        public decimal PermissionLevelId { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }

        public UserGroupDAL Group { get; set; }
        public PermissionLevelDAL PermissionLevel { get; set; }
        public UserDAL User { get; set; }
        public BackendActionDAL Action { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
    }
        public class BackendActionPermissionDALConfiguration : IEntityTypeConfiguration<BackendActionPermissionDAL>
        {
            public void Configure(EntityTypeBuilder<BackendActionPermissionDAL> builder)
            {
                builder.ToTable("ACTION_ACCESS");

                builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.PermissionLevelId).HasColumnName("PERMISSION_LEVEL_ID");
				builder.Property(x => x.GroupId).HasColumnName("GROUP_ID");
				builder.Property(x => x.UserId).HasColumnName("USER_ID");
				builder.Property(x => x.ActionId).HasColumnName("ACTION_ID");
				builder.Property(x => x.Id).HasColumnName("ACTION_ACCESS_ID").ValueGeneratedOnAdd();


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Group).WithMany(y => y.ACTION_ACCESS).HasForeignKey(x => x.GroupId);
    			builder.HasOne(x => x.PermissionLevel).WithMany(y => y.ACTION_ACCESS).HasForeignKey(x => x.PermissionLevelId);
    			builder.HasOne(x => x.User).WithMany(y => y.ACTION_ACCESS).HasForeignKey(x => x.UserId);
    			builder.HasOne(x => x.Action).WithMany(y => y.ACTION_ACCESS).HasForeignKey(x => x.ActionId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.ACTION_ACCESS_1).HasForeignKey(x => x.LastModifiedBy);

            }
        }
}
