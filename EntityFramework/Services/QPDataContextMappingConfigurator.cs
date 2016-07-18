using Quantumart.QP8.EntityFramework.Models;
using Quantumart.QPublishing.Info;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
{
    public abstract class QPDataContextMappingConfiguratorBase : IMappingConfigurator
    {
        public static ContentAccess DefaultContentAccess = ContentAccess.Live;
        private ContentAccess _contentAccess;
        private static ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>> _cache = new ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>>();

        public QPDataContextMappingConfiguratorBase()
            : this(DefaultContentAccess)
        { }


        public QPDataContextMappingConfiguratorBase(ContentAccess contentAccess)
        {
            _contentAccess = contentAccess;
        }

        public virtual DbCompiledModel GetBuiltModel(DbConnection connection)
        {
            return _cache.GetOrAdd(_contentAccess, a =>
            {
                var builder = new DbModelBuilder();
                OnModelCreating(builder);
                var builtModel = builder.Build(connection);

                return new Lazy<DbCompiledModel>(
                    () => builtModel.Compile(),
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }).Value;
        }


        //HINT: use SystemColumnNames
        public abstract void OnModelCreating(DbModelBuilder modelBuilder);

        #region Private members
        private string GetTableName(string contentId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("content_{0}_live_new", contentId);
                case ContentAccess.Stage:
                    return string.Format("content_{0}_stage_new", contentId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("content_{0}_united_new", contentId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        private string GetLinkTableName(string linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united", linkId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("item_link_{0}_united", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        private string GetReversedLinkTableName(string linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}_rev", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united_rev", linkId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("item_link_{0}_united_rev", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }
        #endregion

        #region Nested type
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
            /// Splitted versions of articles including invisible and archived
            /// </summary>
            InvisibleOrArchived = 2
        }
        #endregion
    }
}
