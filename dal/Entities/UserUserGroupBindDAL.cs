using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{
    public class UserUserGroupBindDAL
    {
        public decimal UserId { get; set; }
        public decimal UserGroupId { get; set; }

        public UserDAL User { get; set; }
        public UserGroupDAL UserGroup { get; set; }
    }

    public class UserUserGroupBindDALConfiguration : IEntityTypeConfiguration<UserUserGroupBindDAL>
    {
        public void Configure(EntityTypeBuilder<UserUserGroupBindDAL> builder)
        {
            builder.ToTable("USER_GROUP_BIND");

            builder.Property(x => x.UserId).HasColumnName("USER_ID");
            builder.Property(x => x.UserGroupId).HasColumnName("GROUP_ID");

            builder.HasKey(x => new { x.UserId, x.UserGroupId });

            builder
                .HasOne(x => x.User)
                .WithMany(x => x.UserGroupBinds)
                .HasForeignKey(x => x.UserId);

            builder
                .HasOne(x => x.UserGroup)
                .WithMany(x => x.UserGroupBinds)
                .HasForeignKey(x => x.UserGroupId);
        }
    }
}
