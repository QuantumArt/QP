namespace QA.EF.CodeFirstV6.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        static Model1()
        {
            Database.SetInitializer<Model1>(new NullDatabaseInitializer<Model1>());
        }

        public Model1()
            : base("name=Model1")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<MarketingProduct> MarketingProducts { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<StatusType> StatusTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var mapping = new Model1MappingConfigurator();
            mapping.OnModelCreating(modelBuilder);
        }
    }
}
