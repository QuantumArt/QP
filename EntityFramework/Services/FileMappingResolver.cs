using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QP8.CodeGeneration.Services;
using System.Data.Common;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class FileMappingResolver : IMappingResolver
    {
        private readonly ModelReader _model;
        private readonly string _path;

        public FileMappingResolver(string path)
        {
            _path = path;
            _model = new ModelReader(path, _ => { });
        }

        #region IMappingResolver implementation
        public object GetCacheKey()
        {
            return _path;
        }

        public void Initialize(DbConnection connection)
        {
        }

        public AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName)
        {
            var attributes = from c in _model.Contents
                             from a in c.Attributes
                             where
                                 c.MappedName == contentMappedName &&
                                 a.MappedName == fieldMappedName
                             select a;

            return attributes.Single();
        }

        public ContentInfo GetContent(string mappedName)
        {
            return _model.Contents.Single(c => c.MappedName == mappedName);
        }    
        #endregion
    }
}
