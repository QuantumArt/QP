using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;	


namespace EntityFramework6.Test.DataContext
{
	public partial class EF6Model : DbContext
	{
		public static ContentAccess DefaultContentAccess = ContentAccess.Live;

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

        public virtual DbSet<AfiellFieldsItem> AfiellFieldsItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
		    var schemaProvider = new StaticSchemaProvider();
		    var mapping = new MappingConfigurator(DefaultContentAccess, schemaProvider);
            mapping.OnModelCreating(modelBuilder);
        }
	}
}
