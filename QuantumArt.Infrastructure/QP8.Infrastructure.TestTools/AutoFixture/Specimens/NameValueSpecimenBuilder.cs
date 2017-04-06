using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.Data.Extensions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace QP8.Infrastructure.TestTools.AutoFixture.Specimens
{
    public class NameValueSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var propertyInfo = request as PropertyInfo;
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(NameValueCollection))
            {
                var nvc = new NameValueCollection();
                var dictionary = context.Create<Dictionary<string, string>>();
                dictionary.ForEach(kvp => nvc.Add(kvp.Key, kvp.Value));
                return nvc;
            }

            return new NoSpecimen();
        }
    }
}
