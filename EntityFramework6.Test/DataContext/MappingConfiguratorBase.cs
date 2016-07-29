using Quantumart.QP8.CodeGeneration.Services;
using Quantumart.QP8.EntityFramework.Models;
//using Quantumart.QPublishing.Info;
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

namespace EntityFramework6.Test.DataContext
{
    public abstract class MappingConfiguratorBase : IMappingConfigurator
    {
        private const string TableLive = "content_{0}_new";
        private const string TableStage = "content_{0}_united_new";
        private const string TableLiveFiltered = "content_{0}_live_new";
        private const string TableStageFiltered = "content_{0}_stage_new";

        public static ContentAccess DefaultContentAccess = ContentAccess.Live;
        protected ContentAccess _contentAccess;
        private static ConcurrentDictionary<object, Lazy<DbCompiledModel>> _cache = new ConcurrentDictionary<object, Lazy<DbCompiledModel>>();  

        #region Constructors
        public MappingConfiguratorBase()
            : this(DefaultContentAccess)
        { }


        public MappingConfiguratorBase(ContentAccess contentAccess)
        {
            _contentAccess = contentAccess;
        }
        #endregion

        #region IMappingConfigurator implementation
        public virtual DbCompiledModel GetBuiltModel(DbConnection connection)
        {
            return _cache.GetOrAdd(GetCacheKey(), a =>
            {
                var builder = new DbModelBuilder();
                OnModelCreating(builder);
                var builtModel = builder.Build(connection);

                return new Lazy<DbCompiledModel>(
                    () => builtModel.Compile(),
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }).Value;
        }

        public virtual void OnModelCreating(DbModelBuilder modelBuilder)
        {
            #region StatusType
            modelBuilder.Entity<StatusType>()
                .ToTable("STATUS_TYPE_NEW")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<StatusType>().Property(e => e.SiteId).HasColumnName("site_id");
            modelBuilder.Entity<StatusType>().Property(e => e.StatusTypeName).HasColumnName("name");
            modelBuilder.Entity<StatusType>().Property(e => e.Weight).HasColumnName("weight");
            #endregion

            #region User
            modelBuilder.Entity<User>()
                .ToTable("USER_NEW")
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<User>().Property(e => e.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<User>().Property(e => e.LastName).HasColumnName("last_name");
            modelBuilder.Entity<User>().Property(e => e.NTLogin).HasColumnName("nt_login");
            modelBuilder.Entity<User>().Property(e => e.ISOCode).HasColumnName("iso_code");
            #endregion

            #region UserGroup
            modelBuilder.Entity<UserGroup>()
                .ToTable("USER_GROUP_NEW")
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");


            modelBuilder.Entity<UserGroup>()
                .HasMany(e => e.Users)
                .WithMany(e => e.UserGroups)
                .Map(m => m.ToTable("USER_GROUP_BIND_NEW").MapLeftKey("GROUP_ID").MapRightKey("USER_ID"));

            #endregion
        }
        #endregion

        #region Protected members
        protected virtual object GetCacheKey()
        {
            return _contentAccess;
        }

        protected string GetTableName(int contentId, bool useDefaultFiltration)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format(useDefaultFiltration ? TableLiveFiltered : TableLive, contentId);
                case ContentAccess.Stage:
                    return string.Format(useDefaultFiltration ? TableStageFiltered : TableStage, contentId);
                case ContentAccess.StageNoDefaultFiltration:
                    return string.Format(TableStage, contentId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        protected string GetLinkTableName(int linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united", linkId);
                case ContentAccess.StageNoDefaultFiltration:
                    return string.Format("item_link_{0}_united", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        protected string GetReversedLinkTableName(int linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}_rev", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united_rev", linkId);
                case ContentAccess.StageNoDefaultFiltration:
                    return string.Format("item_link_{0}_united_rev", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }
        #endregion
    }
}
