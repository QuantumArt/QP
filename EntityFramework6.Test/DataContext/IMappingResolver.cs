using Quantumart.QP8.CodeGeneration.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.DataContext
{
    public interface IMappingResolver
    {
        void Initialize(DbConnection connection);
        object GetCacheKey();
        ContentInfo GetContent(string mappedName);
        AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName);
    }
}