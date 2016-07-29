using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QP8.CodeGeneration.Services;

namespace EntityFramework6.Test.DataContext
{
    public abstract class DynamicMappingConfiguratorBase : MappingConfiguratorBase
    {
        private readonly IMappingResolver _mappingResolver;

        #region Constructors
        public DynamicMappingConfiguratorBase(IMappingResolver mappingResolver)
            : this(mappingResolver, DefaultContentAccess)
        { }

        public DynamicMappingConfiguratorBase(IMappingResolver mappingResolver, ContentAccess contentAccess)
        {
            _mappingResolver = mappingResolver;
            _contentAccess = contentAccess;
        }
        #endregion

        public override DbCompiledModel GetBuiltModel(DbConnection connection)
        {
            _mappingResolver.Initialize(connection);
            return base.GetBuiltModel(connection);
        }

        protected override object GetCacheKey()
        {
            return new { _contentAccess, resolverKey = _mappingResolver.GetCacheKey() };
        }

        #region Dynamic maping
        protected string GetTableName(string mappedName)
        {
            var content = _mappingResolver.GetContent(mappedName);
            return GetTableName(content.Id, content.UseDefaultFiltration);
        }     

        protected string GetLinkTableName(string contentMappedName, string fieldMappedName)
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
    }
}
