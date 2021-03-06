//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public partial class ToolbarButtonDAL
    {

        public int ParentActionId { get; set; }
        public int ActionId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Icon { get; set; }
        public string IconDisabled { get; set; }
        public bool IsCommand { get; set; }

        public BackendActionDAL Action { get; set; }
        public BackendActionDAL ParentAction { get; set; }
    }
        public class ToolbarButtonDALConfiguration : IEntityTypeConfiguration<ToolbarButtonDAL>
        {
            public void Configure(EntityTypeBuilder<ToolbarButtonDAL> builder)
            {
                builder.ToTable("ACTION_TOOLBAR_BUTTON");

                builder.Property(x => x.IsCommand).HasColumnName("IS_COMMAND");
				builder.Property(x => x.IconDisabled).HasColumnName("ICON_DISABLED");
				builder.Property(x => x.Icon).HasColumnName("ICON");
				builder.Property(x => x.Order).HasColumnName("ORDER");
				builder.Property(x => x.Name).HasColumnName("NAME");
				builder.Property(x => x.ActionId).HasColumnName("ACTION_ID");
				builder.Property(x => x.ParentActionId).HasColumnName("PARENT_ACTION_ID");


                builder.HasKey(x => new { x.ParentActionId, x.ActionId });


                builder.HasOne(x => x.Action).WithMany(y => y.ToolbarButtons).HasForeignKey(x => x.ActionId);
    			builder.HasOne(x => x.ParentAction).WithMany().HasForeignKey(x => x.ParentActionId);

            }
        }
}
