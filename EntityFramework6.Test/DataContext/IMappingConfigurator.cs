using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.DataContext
{
    public interface IMappingConfigurator
    {
        DbCompiledModel GetBuiltModel(DbConnection connection);
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}