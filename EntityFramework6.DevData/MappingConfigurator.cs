using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using Quantumart.QP8.CodeGeneration.Services;


namespace Quantumart.QP8.EntityFramework6.DevData
{
    public class MappingConfigurator : MappingConfiguratorBase
    {
        public MappingConfigurator(ContentAccess contentAccess, ISchemaProvider schemaProvider)
            : base(contentAccess, schemaProvider)
        {
        }

        public override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

            #region MarketingProduct mappings
            modelBuilder.Entity<MarketingProduct>()
                .ToTable(GetTableName("MarketingProduct"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingProduct>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.Family_ID)
                .HasColumnName(GetFieldName("MarketingProduct", "Family_ID"));
            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName(GetFieldName("MarketingProduct", "MarketingSign_ID"));
 
            #endregion

            #region Product mappings
            modelBuilder.Entity<Product>()
                .ToTable(GetTableName("Product"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Product>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Product>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Product>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Product>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName(GetFieldName("Product", "MarketingSign_ID"));
            modelBuilder.Entity<Product>()
                .HasOptional<MarketingProduct>(mp => mp.MarketingProduct)
                .WithMany(mp => mp.Products)
                .HasForeignKey(fp => fp.MarketingProduct_ID);

            modelBuilder.Entity<Product>()
                .Property(x => x.MarketingProduct_ID)
                .HasColumnName(GetFieldName("Product", "MarketingProduct"));

            modelBuilder.Entity<Product>().HasMany<Region>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("Product", "Regions"));
                });

            modelBuilder.Entity<Region>().HasMany<Product>(p => p.BackwardForRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("Product", "Regions"));
                });

            modelBuilder.Entity<Product>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<Product>().Ignore(p => p.PDFUploadPath);
 
            #endregion

            #region ProductParameter mappings
            modelBuilder.Entity<ProductParameter>()
                .ToTable(GetTableName("ProductParameter"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProductParameter>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProductParameter>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.GroupMapped_ID)
                .HasColumnName(GetFieldName("ProductParameter", "GroupMapped_ID"));
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.BaseParameter_ID)
                .HasColumnName(GetFieldName("ProductParameter", "BaseParameter_ID"));
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Zone_ID)
                .HasColumnName(GetFieldName("ProductParameter", "Zone_ID"));
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Direction_ID)
                .HasColumnName(GetFieldName("ProductParameter", "Direction_ID"));
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Unit_ID)
                .HasColumnName(GetFieldName("ProductParameter", "Unit_ID"));
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.MatrixParameter_ID)
                .HasColumnName(GetFieldName("ProductParameter", "MatrixParameter_ID"));
            modelBuilder.Entity<ProductParameter>()
                .HasOptional<Product>(mp => mp.Product)
                .WithMany(mp => mp.Parameters)
                .HasForeignKey(fp => fp.Product_ID);

            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Product_ID)
                .HasColumnName(GetFieldName("ProductParameter", "Product"));
 
            #endregion

            #region Region mappings
            modelBuilder.Entity<Region>()
                .ToTable(GetTableName("Region"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Region>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Region>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Region>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Region>()
                .HasOptional<Region>(mp => mp.Parent)
                .WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<Region>()
                .Property(x => x.Parent_ID)
                .HasColumnName(GetFieldName("Region", "Parent"));

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.AllowedRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("Region", "AllowedRegions"));
                });

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.BackwardForAllowedRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName("Region", "AllowedRegions"));
                });


            modelBuilder.Entity<Region>().HasMany<Region>(p => p.DeniedRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("Region", "DeniedRegions"));
                });

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.BackwardForDeniedRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName("Region", "DeniedRegions"));
                });

 
            #endregion

            #region MobileTariff mappings
            modelBuilder.Entity<MobileTariff>()
                .ToTable(GetTableName("MobileTariff"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileTariff>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MobileTariff>()
                .HasOptional<Product>(mp => mp.Product)
                .WithMany(mp => mp.MobileTariffs)
                .HasForeignKey(fp => fp.Product_ID);

            modelBuilder.Entity<MobileTariff>()
                .Property(x => x.Product_ID)
                .HasColumnName(GetFieldName("MobileTariff", "Product"));
 
            #endregion

            #region Setting mappings
            modelBuilder.Entity<Setting>()
                .ToTable(GetTableName("Setting"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Setting>()
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Setting>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Setting>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Setting>()
                .Property(x => x.ValueMapped)
                .HasColumnName(GetFieldName("Setting", "ValueMapped"));

            modelBuilder.Entity<Setting>().HasMany<Setting>(p => p.RelatedSettings).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("Setting", "RelatedSettings"));
                });

            modelBuilder.Entity<Setting>().HasMany<Setting>(p => p.BackwardForRelatedSettings).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName("Setting", "RelatedSettings"));
                });

 
            #endregion
        }
    }
}
