using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities;

public class BackendActionLogUserGroupDAL
{
    public int Id { get; set; }
    public int BackendActionLogId { get; set; }
    public decimal GroupId { get; set; }

    public BackendActionLogDAL ActionLog { get; set; }
}

public class BackendActionLogUserRoleDALConfiguration : IEntityTypeConfiguration<BackendActionLogUserGroupDAL>
{
    public void Configure(EntityTypeBuilder<BackendActionLogUserGroupDAL> builder)
    {
        builder.ToTable("backend_action_log_user_groups").HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.BackendActionLogId).HasColumnName("backend_action_log_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");

        builder.HasOne(x => x.ActionLog)
            .WithMany(x => x.UserGroups)
            .HasForeignKey(x => x.BackendActionLogId)
            .IsRequired();
    }
}
