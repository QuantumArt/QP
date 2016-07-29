using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.Tests
{
    public class MappingValuesAttribute : ValuesAttribute
    {
        public MappingValuesAttribute()
            : base(
                Mapping.StaticMapping,
                Mapping.FileDefaultMapping,
                Mapping.FileDynamicMapping,
                Mapping.DatabaseDefaultMapping,
                Mapping.DatabaseDynamicMapping)
        {

        }
    }
}
