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
    public class EF6ModelMappingConfigurator : MappingConfiguratorBase
    {	
        public EF6ModelMappingConfigurator()
            : base()
        {
		}

        public EF6ModelMappingConfigurator(ContentAccess contentAccess)
            : base(contentAccess)
        {
		}	
       
        public override void OnModelCreating(DbModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);

            #region MarketingProduct mappings
            modelBuilder.Entity<MarketingProduct>()
                .ToTable(GetTableName(287, true))
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
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingProduct>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
 
            #endregion

            #region Product mappings
            modelBuilder.Entity<Product>()
                .ToTable(GetTableName(288, true))
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
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<Product>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<Product>()
                .HasOptional<MarketingProduct>(mp => mp.MarketingProduct)
                .WithMany(mp => mp.Products)
                .HasForeignKey(fp => fp.MarketingProduct_ID);

            modelBuilder.Entity<Product>()
                .Property(x => x.MarketingProduct_ID)
                .HasColumnName("MarketingProduct");

            modelBuilder.Entity<Product>().HasMany<Region>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName(21));
                });

            modelBuilder.Entity<Region>().HasMany<Product>(p => p.BackwardForRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName(21));
                });

            modelBuilder.Entity<Product>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<Product>().Ignore(p => p.PDFUploadPath);
 
            #endregion

            #region ProductParameter mappings
            modelBuilder.Entity<ProductParameter>()
                .ToTable(GetTableName(291, true))
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
                .Property(x => x.Group_ID)
                .HasColumnName("Group");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.BaseParameter_ID)
                .HasColumnName("BaseParameter");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Zone_ID)
                .HasColumnName("Zone");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Direction_ID)
                .HasColumnName("Direction");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Unit_ID)
                .HasColumnName("Unit");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.MatrixParameter_ID)
                .HasColumnName("MatrixParameter");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.BaseParameter_ID)
                .HasColumnName("BaseParameter");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Zone_ID)
                .HasColumnName("Zone");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Direction_ID)
                .HasColumnName("Direction");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Unit_ID)
                .HasColumnName("Unit");
            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.MatrixParameter_ID)
                .HasColumnName("MatrixParameter");
            modelBuilder.Entity<ProductParameter>()
                .HasOptional<Product>(mp => mp.Product)
                .WithMany(mp => mp.Parameters)
                .HasForeignKey(fp => fp.Product_ID);

            modelBuilder.Entity<ProductParameter>()
                .Property(x => x.Product_ID)
                .HasColumnName("Product");
 
            #endregion

            #region Region mappings
            modelBuilder.Entity<Region>()
                .ToTable(GetTableName(294, true))
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
                .HasColumnName("Parent");

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.AllowedRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName(71));
                });

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.BackwardForAllowedRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName(71));
                });


            modelBuilder.Entity<Region>().HasMany<Region>(p => p.DeniedRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName(72));
                });

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.BackwardForDeniedRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName(72));
                });

 
            #endregion

            #region MobileTariff mappings
            modelBuilder.Entity<MobileTariff>()
                .ToTable(GetTableName(305, true))
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
                .HasColumnName("Product");
 
            #endregion

            #region Setting mappings
            modelBuilder.Entity<Setting>()
                .ToTable(GetTableName(349, true))
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
                .HasColumnName("Value");

            modelBuilder.Entity<Setting>().HasMany<Setting>(p => p.RelatedSettings).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName(69));
                });

            modelBuilder.Entity<Setting>().HasMany<Setting>(p => p.BackwardForRelatedSettings).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName(69));
                });

 
            #endregion
        }
    }
}
