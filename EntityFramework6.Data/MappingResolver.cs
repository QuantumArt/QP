using Quantumart.QP8.CodeGeneration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework6.Data
{
    public interface IMappingResolver
    {
        SchemaInfo GetSchema();
        ContentInfo GetContent(string mappedName);
        AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName);
        AttributeInfo GetAttribute(string key);
    }

    public class MappingResolver : IMappingResolver
    {
        private ModelReader _schema;

        public MappingResolver(ModelReader schema)
        {
            _schema = schema;
        }

        public SchemaInfo GetSchema()
        {
            return _schema.Schema;
        }

        public ContentInfo GetContent(string mappedName)
        {
            return _schema.Contents.Single(c => c.MappedName == mappedName);
        }

        public AttributeInfo GetAttribute(string key)
        {
            var attributes = from c in _schema.Contents
                             from a in c.Attributes
                             where key == c.MappedName + "_" + a.MappedName
                             select a;

            return attributes.Single();
        }
        public AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName)
        {
            var attributes = from c in _schema.Contents
                             from a in c.Attributes
                             where
                                 c.MappedName == contentMappedName &&
                                 a.MappedName == fieldMappedName
                             select a;

            return attributes.Single();
        }
    }
}
