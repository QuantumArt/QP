
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace QA.EF.CodeFirstV6.Codegen.Catalog
{
	public interface IMappingConfigurator
	{
	    DbCompiledModel GetBuiltModel(DbConnection connection);
		void OnModelCreating(DbModelBuilder modelBuilder);
	}
}
