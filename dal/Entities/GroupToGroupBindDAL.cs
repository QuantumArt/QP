using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{
    public class GroupToGroupBindDAL
    {
        public decimal ChildGroupId { get; set; }
        public decimal ParentGroupId { get; set; }

        public UserGroupDAL ChildGroup { get; set; }
        public UserGroupDAL ParentGroup { get; set; }
    }


    public class GroupToGroupBindDALConfiguration : IEntityTypeConfiguration<GroupToGroupBindDAL>
    {
        public void Configure(EntityTypeBuilder<GroupToGroupBindDAL> builder)
        {
            builder.ToTable("Group_To_Group");

            builder.Property(x => x.ChildGroupId).HasColumnName("Child_Group_Id");
            builder.Property(x => x.ParentGroupId).HasColumnName("Parent_Group_Id");

            builder.HasKey(x => new { x.ChildGroupId, x.ParentGroupId });

            builder
                .HasOne(x => x.ChildGroup)
                .WithMany(y => y.ParentGroupToGroupBinds)
                .HasForeignKey(x => x.ChildGroupId);

            builder
                .HasOne(x => x.ParentGroup)
                .WithMany(y => y.ChildGroupToGroupBinds)
                .HasForeignKey(x => x.ParentGroupId);
        }
    }
}
