using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;	


namespace Quantumart.QP8.EntityFramework6.DevData
{
	public partial class EF6Model : DbContext
	{
      partial void OnContextCreated();

        static EF6Model()
        {
            Database.SetInitializer<EF6Model>(new NullDatabaseInitializer<EF6Model>());
        }

        public EF6Model()
            : base("name=EF6Model")
        {
            this.Configuration.LazyLoadingEnabled = true;
			this.Configuration.ProxyCreationEnabled = false;

			OnContextCreated();
        }

        public virtual DbSet<StatusType> StatusTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }					

        public virtual DbSet<MarketingProduct> MarketingProducts { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductParameter> ProductParameters { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<MobileTariff> MobileTariffs { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var mapping = new EF6ModelMappingConfigurator();
            mapping.OnModelCreating(modelBuilder);
        }
	}
}
