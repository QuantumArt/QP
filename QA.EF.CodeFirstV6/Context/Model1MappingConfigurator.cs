using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.EF.CodeFirstV6.Data
{
    public class Model1MappingConfigurator
    {
        public Model1MappingConfigurator()
        {

        }
        public virtual void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Console.WriteLine("OnModelCreating requested");


            #region StatusType
            modelBuilder.Entity<StatusType>()
                .ToTable("STATUS_TYPE_NEW")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<StatusType>().Property(e => e.SiteId).HasColumnName("site_id");
            modelBuilder.Entity<StatusType>().Property(e => e.StatusTypeName).HasColumnName("name");
            modelBuilder.Entity<StatusType>().Property(e => e.Weight).HasColumnName("weight");
            #endregion

            #region User
            modelBuilder.Entity<User>()
                .ToTable("USER_NEW")
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<User>().Property(e => e.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<User>().Property(e => e.LastName).HasColumnName("last_name");
            modelBuilder.Entity<User>().Property(e => e.NTLogin).HasColumnName("nt_login");
            modelBuilder.Entity<User>().Property(e => e.ISOCode).HasColumnName("iso_code");
            #endregion

            #region UserGroup
            modelBuilder.Entity<UserGroup>()
                   .ToTable("USER_GROUP_NEW")
                   .Property(e => e.Id)
                   .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                   .HasColumnName("id");


            modelBuilder.Entity<UserGroup>()
                .HasMany(e => e.Users)
                .WithMany(e => e.UserGroups)
                .Map(m => m.ToTable("USER_GROUP_BIND_NEW").MapLeftKey("GROUP_ID").MapRightKey("USER_ID"));

            #endregion

            #region MarketingProduct
            modelBuilder.Entity<MarketingProduct>()
                .ToTable("content_287_stage_new")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
            #endregion

            #region Product
            modelBuilder.Entity<Product>()
                .ToTable("content_288_stage_new")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");

            modelBuilder.Entity<Product>()
                .Property(x => x.MarketingProduct_ID)
                .HasColumnName("MarketingProduct");

            modelBuilder.Entity<Product>()
                .HasOptional<MarketingProduct>(mp => mp.MarketingProduct)
                .WithMany(mp => mp.Products)
                .HasForeignKey(fp => fp.MarketingProduct_ID);

            modelBuilder.Entity<Product>().HasMany<Region>(p => p.Regions).WithMany()//.WithOptional()
                .Map(rp =>
                {
                    rp.MapLeftKey("item_id");
                    rp.MapRightKey("linked_item_id");
                    rp.ToTable("link_21_new");
                });

            modelBuilder.Entity<Region>().HasMany<Product>(p => p.Products).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("linked_item_id");
                    rp.MapRightKey("item_id");
                    rp.ToTable("link_21_rev_new");
                });


            #endregion

            #region Region
            modelBuilder.Entity<Region>().ToTable("content_294_stage_new").Property(x => x.Id)
                 .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                 .HasColumnName("CONTENT_ITEM_ID");

            modelBuilder.Entity<Region>().Property(x => x.Parent_ID).HasColumnName("Parent");

            modelBuilder.Entity<Region>().HasOptional<Region>(mp => mp.Parent).WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.AllowedRegions).WithMany(r => r.AllowedRegionsBackward)
                .Map(rp =>
                {
                    rp.MapLeftKey("item_id");
                    rp.MapRightKey("linked_item_id");
                    rp.ToTable("link_71_new");
                });

            modelBuilder.Entity<Region>().HasMany<Region>(p => p.DeniedRegions).WithMany(r => r.DeniedRegionsBackward)
                .Map(rp =>
                {
                    rp.MapLeftKey("item_id");
                    rp.MapRightKey("linked_item_id");
                    rp.ToTable("link_72_new");
                });
            #endregion

            #region Setting
            modelBuilder.Entity<Setting>()
                .ToTable("content_349_stage_new")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");

            modelBuilder.Entity<Setting>()
                .HasMany<Setting>(p => p.RelatedSettings)
                .WithMany(r => r.RelatedSettingsBackward)
                .Map(rp =>
                {
                    rp.MapLeftKey("item_id");
                    rp.MapRightKey("linked_item_id");
                    rp.ToTable("link_69_new");
                });
            #endregion

            #region splitted columns
            modelBuilder.Entity<StringDetail>()
                   .ToTable("content_349_stage_new")
                   .Property(x => x.Id)
                   .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                   .HasColumnName("CONTENT_ITEM_ID");

            modelBuilder.Entity<Setting>().HasRequired(e => e.ValueExtended).WithRequiredPrincipal();


            #endregion


            #region Junctions
            modelBuilder.Entity<ProduktyRegionyArticle>()
                .Map(e =>
                {
                    // пока не создадут в БД еще 1 вьюху для развязочной таблицы, этот сценарий не будет работать

                    //e.ToTable("dbo.link_21_new");
                    e.Property(x => x.Product_ID).HasColumnName("item_id");
                    e.Property(x => x.Region_ID).HasColumnName("linked_item_id");
                })
                .HasKey(t => new { t.Product_ID, t.Region_ID });

            modelBuilder.Entity<ProduktyRegionyArticle>()
                .HasRequired<Product>(om => om.Product)
                .WithMany(p => p.ProduktyRegionyArticles)
                .HasForeignKey(p => p.Product_ID);

            modelBuilder.Entity<ProduktyRegionyArticle>()
                .HasRequired<Region>(om => om.Region)
                .WithMany(p => p.ProduktyRegionyArticlesBackward)
                .HasForeignKey(p => p.Region_ID);

            #endregion
        }
    }
}
