using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL.Entities
{

    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class BackendActionDAL
    {

        public int Id { get; set; }  //+
        public int TypeId { get; set; } //+
        public int EntityTypeId { get; set; } //+
        public string Name { get; set; } //+
        public string ShortName { get; set; } //+
        public string Code { get; set; } //+
        public string UserControlFile { get; set; }//+
        public string ControllerActionUrl { get; set; } //+
        public string ConfirmPhrase { get; set; } //+
        public bool IsInterface { get; set; } //+
        public bool HasPreAction { get; set; } //+
        public int? ParentId { get; set; } //+
        public bool IsWindow { get; set; } //+
        public int? WindowWidth { get; set; }//+
        public int? WindowHeight { get; set; }//+
        public int? DefaultViewTypeId { get; set; } //+
        public bool AllowSearch { get; set; } //+
        public bool AllowPreview { get; set; } //+
        public int? NextSuccessfulActionId { get; set; } //+
        public int? NextFailedActionId { get; set; } //+
        public bool IsCustom { get; set; } //+
        public decimal? TabId { get; set; } //+
        public bool IsMultistep { get; set; } //+
        public bool? HasSettings { get; set; } //+
        public string AdditionalControllerActionUrl { get; set; } //+
        public int? EntityLimit { get; set; } //+

        public ActionTypeDAL ActionType { get; set; }
        public ICollection<BackendActionDAL> ChildActions { get; set; }
        public BackendActionDAL ParentAction { get; set; }
        public EntityTypeDAL EntityType { get; set; }
        public ViewTypeDAL DefaultViewType { get; set; }
        public ICollection<ActionViewDAL> Views { get; set; }
        public ICollection<BackendActionDAL> ParentPreFailedActions { get; set; }
        public BackendActionDAL NextFailedAction { get; set; }
        public ICollection<BackendActionDAL> ParentPreSuccessfulActions { get; set; }
        public BackendActionDAL NextSuccessfulAction { get; set; }
        public ICollection<CustomActionDAL> CustomActions { get; set; }
        public ICollection<ToolbarButtonDAL> ToolbarButtons { get; set; }
        public ICollection<ContextMenuItemDAL> ContextMenuItems { get; set; }
        public ICollection<BackendActionPermissionDAL> ACTION_ACCESS { get; set; }

        public ICollection<ActionExclusionsBindDAL> ExcludesBinds { get; set; }
        public ICollection<ActionExclusionsBindDAL> ExcludedByBinds { get; set; }

        [NotMapped]
        public IEnumerable<BackendActionDAL> Excludes => ExcludesBinds.Select(x => x.Excludes);

        [NotMapped]
        public IEnumerable<BackendActionDAL> ExcludedBy => ExcludedByBinds.Select(x => x.ExcludedBy);

    }
        public class BackendActionDALConfiguration : IEntityTypeConfiguration<BackendActionDAL>
        {
            public void Configure(EntityTypeBuilder<BackendActionDAL> builder)
            {
                builder.ToTable("BACKEND_ACTION");

                builder.Property(x => x.EntityLimit).HasColumnName("ENTITY_LIMIT"); //+
				builder.Property(x => x.AdditionalControllerActionUrl).HasColumnName("ADDITIONAL_CONTROLLER_ACTION_URL"); //+
				builder.Property(x => x.HasSettings).HasColumnName("HAS_SETTINGS"); //+
				builder.Property(x => x.IsMultistep).HasColumnName("IS_MULTISTEP"); //+
				builder.Property(x => x.TabId).HasColumnName("TAB_ID"); //+
				builder.Property(x => x.IsCustom).HasColumnName("IS_CUSTOM"); //+
				builder.Property(x => x.Id).HasColumnName("ID"); //+
				builder.Property(x => x.TypeId).HasColumnName("TYPE_ID"); //+
				builder.Property(x => x.EntityTypeId).HasColumnName("ENTITY_TYPE_ID"); //+
				builder.Property(x => x.Name).HasColumnName("NAME"); //+
				builder.Property(x => x.ShortName).HasColumnName("SHORT_NAME"); //+
				builder.Property(x => x.Code).HasColumnName("CODE"); //+
				builder.Property(x => x.UserControlFile).HasColumnName("USER_CONTROL_FILE"); //+
				builder.Property(x => x.ControllerActionUrl).HasColumnName("CONTROLLER_ACTION_URL"); //+
				builder.Property(x => x.ConfirmPhrase).HasColumnName("CONFIRM_PHRASE"); //+
				builder.Property(x => x.IsInterface).HasColumnName("IS_INTERFACE"); //+
				builder.Property(x => x.HasPreAction).HasColumnName("HAS_PRE_ACTION"); //+
				builder.Property(x => x.WindowHeight).HasColumnName("WINDOW_HEIGHT"); //+
				builder.Property(x => x.WindowWidth).HasColumnName("WINDOW_WIDTH");//+
				builder.Property(x => x.IsWindow).HasColumnName("IS_WINDOW"); //+
				builder.Property(x => x.ParentId).HasColumnName("PARENT_ID"); //+
				builder.Property(x => x.DefaultViewTypeId).HasColumnName("DEFAULT_VIEW_TYPE_ID"); //+
				builder.Property(x => x.AllowSearch).HasColumnName("ALLOW_SEARCH"); //+
				builder.Property(x => x.AllowPreview).HasColumnName("ALLOW_PREVIEW"); //+
				builder.Property(x => x.NextSuccessfulActionId).HasColumnName("NEXT_SUCCESSFUL_ACTION_ID"); //+
				builder.Property(x => x.NextFailedActionId).HasColumnName("NEXT_FAILED_ACTION_ID"); //+


                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.ActionType).WithMany(y => y.BackendAction).HasForeignKey(x => x.TypeId);
    			builder.HasMany(x => x.ChildActions).WithOne(y => y.ParentAction).HasForeignKey(y => y.ParentId);
    			builder.HasOne(x => x.ParentAction).WithMany(y => y.ChildActions).HasForeignKey(x => x.ParentId);
    			builder.HasOne(x => x.EntityType).WithMany(y => y.Actions).HasForeignKey(x => x.EntityTypeId);
    			builder.HasOne(x => x.DefaultViewType).WithMany().HasForeignKey(x => x.DefaultViewTypeId);
    			builder.HasMany(x => x.Views).WithOne(y => y.BackendAction).HasForeignKey(y => y.ActionId);
    			builder.HasMany(x => x.ParentPreFailedActions).WithOne(y => y.NextFailedAction).HasForeignKey(y => y.NextFailedActionId);
    			builder.HasOne(x => x.NextFailedAction).WithMany(y => y.ParentPreFailedActions).HasForeignKey(x => x.NextFailedActionId);
    			builder.HasMany(x => x.ParentPreSuccessfulActions).WithOne(y => y.NextSuccessfulAction).HasForeignKey(y => y.NextSuccessfulActionId);
    			builder.HasOne(x => x.NextSuccessfulAction).WithMany(y => y.ParentPreSuccessfulActions).HasForeignKey(x => x.NextSuccessfulActionId);
    			builder.HasMany(x => x.CustomActions).WithOne(y => y.Action).HasForeignKey(y => y.ActionId);
    			builder.HasMany(x => x.ToolbarButtons).WithOne(y => y.Action).HasForeignKey(y => y.ActionId);
    			builder.HasMany(x => x.ContextMenuItems).WithOne(y => y.Action).HasForeignKey(y => y.ActionId);
    			builder.HasMany(x => x.ACTION_ACCESS).WithOne(y => y.Action).HasForeignKey(y => y.ActionId);

            }
        }
}
