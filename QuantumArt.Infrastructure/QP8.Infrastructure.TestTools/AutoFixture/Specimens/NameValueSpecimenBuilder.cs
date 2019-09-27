using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace QP8.Infrastructure.TestTools.AutoFixture.Specimens
{
    [DebuggerStepThrough]
    public class NameValueSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo propertyInfo && propertyInfo.PropertyType == typeof(Dictionary<string, StringValues>))
            {
                var nvc = new Dictionary<string, StringValues>();
                var dictionary = context.Create<Dictionary<string, string>>();
                foreach (var kvp in dictionary)
                {
                    nvc.Add(kvp.Key, kvp.Value);
                }

                return nvc;
            }

            return new NoSpecimen();
        }
    }
}
