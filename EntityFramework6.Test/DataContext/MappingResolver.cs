using Quantumart.QP8.CodeGeneration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.DataContext
{
    public interface IMappingResolver
    {
        ContentInfo GetContent(string mappedName);
        AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName);
    }

    public class MappingResolver : IMappingResolver
    {
        private ModelReader _schema;

        public MappingResolver(ModelReader schema)
        {
            _schema = schema;
        }

        public ContentInfo GetContent(string mappedName)
        {
            return _schema.Contents.Single(c => c.MappedName == mappedName);
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
