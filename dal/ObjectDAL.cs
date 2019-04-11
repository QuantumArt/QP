using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quantumart.QP8.DAL
{
    // ReSharper disable CollectionNeverUpdated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class ObjectDAL : IQpEntityObject
    {
        public decimal Id { get; set; }
        public decimal? ParentObjectId { get; set; }
        public decimal? PageTemplateId { get; set; }
        public decimal? PageId { get; set; }
        public string Name { get; set; }
        public decimal? DefaultFormatId { get; set; }
        public string Description { get; set; }
        public decimal TypeId { get; set; }
        public decimal UseDefaultValues { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
        public decimal AllowStageEdit { get; set; }
        public bool Global { get; set; }
        public string NetName { get; set; }
        public DateTime? Locked { get; set; }
        public decimal? LockedBy { get; set; }
        public bool EnableViewstate { get; set; }
        public string ControlCustomClass { get; set; }
        public bool DisableDatabind { get; set; }
        public bool PermanentLock { get; set; }

        public ContainerDAL Container { get; set; }
        public ContentFormDAL ContentForm { get; set; }
        public ICollection<ObjectFormatDAL> ChildObjectFormats { get; set; }
        public UserDAL LockedByUser { get; set; }
        public ICollection<ObjectDAL> InheritedObjects { get; set; }
        public ObjectDAL ObjectInheritedFrom { get; set; }
        public ObjectTypeDAL ObjectType { get; set; }
        public PageDAL Page { get; set; }
        public PageTemplateDAL PageTemplate { get; set; }
        public ICollection<ObjectValuesDAL> ObjectValues { get; set; }
        public ObjectFormatDAL DefaultFormat { get; set; }

        public UserDAL LastModifiedByUser { get; set; }
        public ICollection<ObjectStatusTypeBindDAL> ObjectStatusTypeBinds { get; set; }

        [NotMapped]
        public IEnumerable<StatusTypeDAL> StatusType => ObjectStatusTypeBinds?.Select(x => x.StatusType);
    }

    public class ObjectDALConfiguration : IEntityTypeConfiguration<ObjectDAL>
    {
        public void Configure(EntityTypeBuilder<ObjectDAL> builder)
        {
            builder.ToTable("OBJECT");

            builder.Property(x => x.PermanentLock).HasColumnName("PERMANENT_LOCK");
            builder.Property(x => x.Id).HasColumnName("OBJECT_ID");
            builder.Property(x => x.ParentObjectId).HasColumnName("PARENT_OBJECT_ID");
            builder.Property(x => x.PageTemplateId).HasColumnName("PAGE_TEMPLATE_ID");
            builder.Property(x => x.PageId).HasColumnName("PAGE_ID");
            builder.Property(x => x.Name).HasColumnName("OBJECT_NAME");
            builder.Property(x => x.DefaultFormatId).HasColumnName("OBJECT_FORMAT_ID");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
            builder.Property(x => x.TypeId).HasColumnName("OBJECT_TYPE_ID");
            builder.Property(x => x.UseDefaultValues).HasColumnName("USE_DEFAULT_VALUES");
            builder.Property(x => x.Created).HasColumnName("CREATED");
            builder.Property(x => x.Modified).HasColumnName("MODIFIED");
            builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
            builder.Property(x => x.AllowStageEdit).HasColumnName("allow_stage_edit");
            builder.Property(x => x.Global).HasColumnName("GLOBAL");
            builder.Property(x => x.NetName).HasColumnName("NET_OBJECT_NAME");
            builder.Property(x => x.Locked).HasColumnName("LOCKED");
            builder.Property(x => x.LockedBy).HasColumnName("LOCKED_BY");
            builder.Property(x => x.EnableViewstate).HasColumnName("ENABLE_VIEWSTATE");
            builder.Property(x => x.ControlCustomClass).HasColumnName("control_custom_class");
            builder.Property(x => x.DisableDatabind).HasColumnName("DISABLE_DATABIND");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Container).WithOne(y => y.Object).HasForeignKey<ContainerDAL>(y => y.ObjectId);
            builder.HasOne(x => x.ContentForm).WithOne(y => y.Object).HasForeignKey<ContentFormDAL>(y => y.ObjectId);
            builder.HasMany(x => x.ChildObjectFormats).WithOne(y => y.Object).HasForeignKey(y => y.ObjectId);
            builder.HasOne(x => x.LockedByUser).WithMany(y => y.Object).HasForeignKey(x => x.LockedBy);
            builder.HasMany(x => x.InheritedObjects).WithOne(y => y.ObjectInheritedFrom).HasForeignKey(y => y.ParentObjectId);
            builder.HasOne(x => x.ObjectInheritedFrom).WithMany(y => y.InheritedObjects).HasForeignKey(x => x.ParentObjectId);
            builder.HasOne(x => x.ObjectType).WithMany(y => y.Object).HasForeignKey(x => x.TypeId);
            builder.HasOne(x => x.Page).WithMany(y => y.Object).HasForeignKey(x => x.PageId);
            builder.HasOne(x => x.PageTemplate).WithMany(y => y.Object).HasForeignKey(x => x.PageTemplateId);
            builder.HasMany(x => x.ObjectValues).WithOne(y => y.Object).HasForeignKey(y => y.ObjectId);
            builder.HasOne(x => x.DefaultFormat).WithMany(y => y.Object1).HasForeignKey(x => x.DefaultFormatId);
            
            builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.OBJECT).HasForeignKey(x => x.LastModifiedBy);

            builder.HasMany(x => x.ObjectStatusTypeBinds).WithOne(y => y.Object).HasForeignKey(y => y.ObjectId);

            // builder.HasMany(x => x.StatusType).WithMany(y => y.Objects).HasForeignKey(y => y.);
        }
    }
}
