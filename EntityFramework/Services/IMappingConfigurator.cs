using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
{
    public interface IMappingConfigurator
    {
        DbCompiledModel GetBuiltModel(DbConnection connection);
        void OnModelCreating(DbModelBuilder modelBuilder);
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