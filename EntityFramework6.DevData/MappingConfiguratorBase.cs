using Quantumart.QP8.EntityFramework.Models;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Threading;

namespace Quantumart.QP8.EntityFramework6.DevData
{
    public abstract class MappingConfiguratorBase : IMappingConfigurator
    {
        private const string TableLive = "content_{0}_new";
        private const string TableStage = "content_{0}_united_new";
        private const string TableLiveFiltered = "content_{0}_live_new";
        private const string TableStageFiltered = "content_{0}_stage_new";

        private static ConcurrentDictionary<object, Lazy<MappingInfo>> _cache = new ConcurrentDictionary<object, Lazy<MappingInfo>>();

        private readonly ISchemaProvider _schemaProvider;
        private readonly ContentAccess _contentAccess;
        private IMappingResolver _mappingResolver;

        public MappingConfiguratorBase(ContentAccess contentAccess, ISchemaProvider schemaProvider)
        {
            _contentAccess = contentAccess;
            _schemaProvider = schemaProvider;
        }

        public virtual MappingInfo GetMappingInfo(DbConnection connection)
        {
            return _cache.GetOrAdd(GetCacheKey(), a =>
            {
                var _schema = _schemaProvider.GetSchema();
                _mappingResolver = new MappingResolver(_schema);

                var builder = new DbModelBuilder();
                OnModelCreating(builder);
                var builtModel = builder.Build(connection);

                return new Lazy<MappingInfo>(
                    () => new MappingInfo(builtModel.Compile(), _schema),
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

        private object GetCacheKey()
        {
            return new { _contentAccess, resolverKey = _schemaProvider.GetCacheKey() };
        }

        #region Dynamic mapping
        protected string GetFieldName(string contentMappedName, string fieldMappedName)
        {
            return _mappingResolver.GetAttribute(contentMappedName, fieldMappedName).Name;
        }

        protected string GetTableName(string mappedName)
        {
            var content = _mappingResolver.GetContent(mappedName);
            return GetTableName(content.Id, content.UseDefaultFiltration);
        }

        protected string GetLinkTableName( string contentMappedName, string fieldMappedName)
        {
            int linkId = _mappingResolver.GetAttribute(contentMappedName, fieldMappedName).LinkId;
            return GetLinkTableName(linkId);
        }

        protected string GetReversedLinkTableName(string contentMappedName, string fieldMappedName)
        {
            int linkId = _mappingResolver.GetAttribute(contentMappedName, fieldMappedName).LinkId;
            return GetReversedLinkTableName(linkId);
        }
        #endregion

        #region Static mapping
        private string GetTableName(int contentId, bool useDefaultFiltration)
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

        private string GetLinkTableName(int linkId)
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

        private string GetReversedLinkTableName(int linkId)
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
