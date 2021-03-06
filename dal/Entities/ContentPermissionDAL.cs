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
    public partial class ContentPermissionDAL :  IQpEntityObject
    {

        public decimal ContentId { get; set; }
        public Nullable<decimal> UserId { get; set; }
        public Nullable<decimal> GroupId { get; set; }
        public decimal PermissionLevelId { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal PropagateToItems { get; set; }
        public decimal Id { get; set; }
        public bool Hide { get; set; }

        public ContentDAL Content { get; set; }
        public UserGroupDAL Group { get; set; }
        public UserDAL User { get; set; }
        public PermissionLevelDAL PermissionLevel { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
    }
        public class ContentPermissionDALConfiguration : IEntityTypeConfiguration<ContentPermissionDAL>
        {
            public void Configure(EntityTypeBuilder<ContentPermissionDAL> builder)
            {
                builder.ToTable("CONTENT_ACCESS");

                builder.Property(x => x.Hide).HasColumnName("HIDE");
				builder.Property(x => x.ContentId).HasColumnName("CONTENT_ID");
				builder.Property(x => x.UserId).HasColumnName("USER_ID");
				builder.Property(x => x.GroupId).HasColumnName("GROUP_ID");
				builder.Property(x => x.PermissionLevelId).HasColumnName("PERMISSION_LEVEL_ID");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				builder.Property(x => x.PropagateToItems).HasColumnName("propagate_to_items");
				builder.Property(x => x.Id).HasColumnName("CONTENT_ACCESS_ID").ValueGeneratedOnAdd();


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Content).WithMany(y => y.AccessRules).HasForeignKey(x => x.ContentId);
    			builder.HasOne(x => x.Group).WithMany(y => y.ContentAccess).HasForeignKey(x => x.GroupId);
    			builder.HasOne(x => x.User).WithMany(y => y.ContentAccess).HasForeignKey(x => x.UserId);
    			builder.HasOne(x => x.PermissionLevel).WithMany(y => y.CONTENT_ACCESS).HasForeignKey(x => x.PermissionLevelId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedBy);

            }
        }
}
