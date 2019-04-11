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
    public partial class SiteFolderDAL :  IQpEntityObject
    {
    
        public decimal SiteId { get; set; }
        public decimal Id { get; set; }
        public Nullable<decimal> ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Filter { get; set; }
        public string Path { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Modified { get; set; }
        public decimal LastModifiedBy { get; set; }
    
        public ICollection<SiteFolderPermissionDAL> FolderAccess { get; set; }
        public ICollection<SiteFolderDAL> Folder1 { get; set; }
        public SiteFolderDAL Folder2 { get; set; }
        public SiteDAL Site { get; set; }
        public UserDAL LastModifiedByUser { get; set; }
    }
        public class SiteFolderDALConfiguration : IEntityTypeConfiguration<SiteFolderDAL>
        {
            public void Configure(EntityTypeBuilder<SiteFolderDAL> builder)
            {
                builder.ToTable("FOLDER");
    
                builder.Property(x => x.SiteId).HasColumnName("SITE_ID");
				builder.Property(x => x.Id).HasColumnName("FOLDER_ID");
				builder.Property(x => x.ParentId).HasColumnName("PARENT_FOLDER_ID");
				builder.Property(x => x.Name).HasColumnName("NAME");
				builder.Property(x => x.Description).HasColumnName("DESCRIPTION");
				builder.Property(x => x.Filter).HasColumnName("FILTER");
				builder.Property(x => x.Path).HasColumnName("PATH");
				builder.Property(x => x.Created).HasColumnName("CREATED");
				builder.Property(x => x.Modified).HasColumnName("MODIFIED");
				builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY");
				
    
                builder.HasKey(x => x.Id);
    
                builder.HasMany(x => x.FolderAccess).WithOne(y => y.Folder).HasForeignKey(y => y.FolderId);
    			builder.HasMany(x => x.Folder1).WithOne(y => y.Folder2).HasForeignKey(y => y.ParentId);
    			builder.HasOne(x => x.Folder2).WithMany(y => y.Folder1).HasForeignKey(x => x.ParentId);
    			builder.HasOne(x => x.Site).WithMany(y => y.Folders).HasForeignKey(x => x.SiteId);
    			builder.HasOne(x => x.LastModifiedByUser).WithMany(y => y.FOLDER).HasForeignKey(x => x.LastModifiedBy);
    			
            }
        }
}
