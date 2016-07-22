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

namespace Quantumart.QP8.EntityFramework.Services
{
    public abstract class MappingConfiguratorBase : IMappingConfigurator
    {
        public static ContentAccess DefaultContentAccess = ContentAccess.Live;
        protected ContentAccess _contentAccess;
        private static ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>> _cache = new ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>>();

        private ModelReader _dynamicModel;
        protected ModelReader DynamicModel
        {
            get
            {
                if (_dynamicModel == null)
                {
                    _dynamicModel = GetDynamicModel();
                }

                return _dynamicModel;
            }
        }

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
        protected abstract ModelReader GetDynamicModel();

        #region Dynamic maping
        protected string GetTableName(string mappedName)
        {
            var content = DynamicModel.Contents.Single(c => c.MappedName == mappedName);
            return GetTableName(content.Id, content.UseDefaultFiltration);
        }

        private int GetLinkId(string contentMappedName, string fieldMappedName)
        {
            var linkIds = from c in DynamicModel.Contents
                          from f in c.Attributes
                          where
                              c.MappedName == contentMappedName &&
                              f.MappedName == fieldMappedName
                          select f.LinkId;

            return linkIds.Single();
        }

        protected string GetLinkTableName(string contentMappedName, string fieldMappedName)
        {
            int linkId = GetLinkId(contentMappedName, fieldMappedName);
            return GetLinkTableName(linkId);
        }

        protected string GetReversedLinkTableName(string contentMappedName, string fieldMappedName)
        {
            int linkId = GetLinkId(contentMappedName, fieldMappedName);
            return GetReversedLinkTableName(linkId);
        }
        #endregion

        #region Static mapinng
        protected string GetTableName(int contentId, bool useDefaultFiltration)
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

        protected string GetLinkTableName(int linkId)
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

        protected string GetReversedLinkTableName(int linkId)
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
