using EntityFramework6.Test.DataContext;
using NUnit.Framework;

namespace EntityFramework6.Test.Infrastructure
{
    public class ContentAccessValuesAttribute : ValuesAttribute
    {
        public ContentAccessValuesAttribute()
            : base(
                ContentAccess.Live,
                ContentAccess.Stage,
                ContentAccess.StageNoDefaultFiltration)
        {
        }
    }
}
