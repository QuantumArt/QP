using NUnit.Framework;

namespace EntityFramework6.Test.Infrastructure
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
