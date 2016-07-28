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
        ContentInfo GetContent(string mappedName);
        AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName);
    }
}