using Quantumart.QP8.CodeGeneration.Services;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Quantumart.QP8.EntityFramework6.Data
{
    public interface IMappingConfigurator
    {
        MappingInfo GetMappingInfo(DbConnection connection);
        void OnModelCreating(DbModelBuilder modelBuilder);
    }

    public class MappingInfo
    {
        public DbCompiledModel DbCompiledModel { get; private set; }
        public ModelReader Schema { get; private set; }

        public MappingInfo(DbCompiledModel dbCompiledModel, ModelReader schema)
        {
            DbCompiledModel = dbCompiledModel;
            Schema = schema;
        }
    }
    public enum ContentAccess
    {
        /// <summary>
        /// Published articles
        /// </summary>
        Live = 0,
        /// <summary>
        /// Splitted versions of articles
        /// </summary>
        Stage = 1,
        /// <summary>
        /// Splitted versions of articles including invisible and archived (overrides useDefaultFiltration content setting)
        /// </summary>
        StageNoDefaultFiltration = 2
    }
}