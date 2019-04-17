using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Quantumart.QP8.DAL
{
    public class NpgSqlQPModelDataContext : QPModelDataContext
    {
        public NpgSqlQPModelDataContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public NpgSqlQPModelDataContext(DbConnection dbConnection) : base(dbConnection)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!string.IsNullOrWhiteSpace(_nameOrConnectionString))
            {
                optionsBuilder.UseNpgsql(_nameOrConnectionString);
            }
            else if (_dbConnection != null)
            {
                optionsBuilder.UseNpgsql(_dbConnection);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            foreach(var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.Relational().TableName.ToSnakeCase();

                foreach(var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Relational().ColumnName.ToSnakeCase();
                }

                foreach(var key in entity.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach(var key in entity.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach(var index in entity.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name.ToSnakeCase();
                }
            }
        }


    }
}
