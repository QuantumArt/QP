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
            else if (_connection != null)
            {
                optionsBuilder.UseNpgsql(_connection);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach(var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToSnakeCase());

                foreach(var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnBaseName().ToSnakeCase());
                }

                foreach(var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                foreach(var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                }

                foreach(var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
                }
            }
        }
    }
}
